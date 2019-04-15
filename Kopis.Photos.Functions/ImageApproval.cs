using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kopis.Photos.Functions
{
	public static class ImageApproval
	{
		private static readonly List<VisualFeatureTypes> analysisFeatures =
			new List<VisualFeatureTypes>()
		{
			VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
			VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
			VisualFeatureTypes.Tags, VisualFeatureTypes.Adult
		};

		[FunctionName("ImageApproval")]
		public static async Task RunOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context)
		{
			var imageName = context.GetInput<string>();
			var imageAnalysis = await context.CallActivityAsync<ImageAnalysis>("ImageAnalysis", imageName);

			// We don't want adult OR racy content
			var approved = !imageAnalysis.Adult.IsAdultContent && !imageAnalysis.Adult.IsRacyContent;

			// If approved, move it to the "processed" container
			if (approved)
			{
				// Move the image
				var finalUrl = await context.CallActivityAsync<string>("MoveImage", imageName);

				// Generate the thumbnail
				var thumbnailUrl = await context.CallActivityAsync<string>("GenerateThumbnail", imageName);

				//Index the metadata
				var imageInfo = new ImageInfo
				{
					Url = finalUrl,
					ThumbnailUrl = thumbnailUrl
				};

				imageInfo.SetAnalysisFields(imageAnalysis);

				var indexed = await context.CallActivityAsync<bool>("IndexImageMetadata", imageInfo);
			}
			else
			{
				// Delete the photo
				var exists = await context.CallActivityAsync<string>("DeletePhoto", imageName);
			}

		}

		[FunctionName("ImageAnalysis")]
		public static async Task<ImageAnalysis> AnalyzeImage([ActivityTrigger] string name, ILogger log)
		{
			log.LogInformation($"Analyzing {name}.");
			ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("CognitiveApiKey")), new System.Net.Http.DelegatingHandler[] { });
			computerVision.Endpoint = Environment.GetEnvironmentVariable("CognitiveApiUrl");

			return await computerVision.AnalyzeImageAsync(await GetBlobAccessUrl("uploaded", name, TimeSpan.FromSeconds(60)), analysisFeatures);
		}

		[FunctionName("MoveImage")]
		public static async Task<string> MoveImage([ActivityTrigger] string name, ILogger log)
		{
			log.LogInformation($"Moving {name}.");
			var uploadedBlob = await GetImageFromContainer("uploaded", name);
			var processedBlob = await GetImageFromContainer("processed", name);

			await processedBlob.StartCopyAsync(uploadedBlob);

			return processedBlob.Uri.ToString();
		}

		[FunctionName("GenerateThumbnail")]
		public static async Task<string> GenerateThumbnail([ActivityTrigger] string name, ILogger log)
		{
			log.LogInformation($"Generating thumbnail for {name}.");
			var thumbnailBlob = await GetImageFromContainer("thumbnails", name);

			ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("CognitiveApiKey")), new System.Net.Http.DelegatingHandler[] { });
			computerVision.Endpoint = Environment.GetEnvironmentVariable("CognitiveApiUrl");

			var thumbnailStream = await computerVision.GenerateThumbnailAsync(200, 200, await GetBlobAccessUrl("processed", name, TimeSpan.FromSeconds(60)), true);

			await thumbnailBlob.UploadFromStreamAsync(thumbnailStream);

			return thumbnailBlob.Uri.ToString();
		}

		[FunctionName("DeletePhoto")]
		public static async Task<bool> DeletePhoto([ActivityTrigger] string name, ILogger log)
		{
			log.LogInformation($"Deleting {name}.");
			var photoBlob = await GetImageFromContainer("uploaded", name);
			var result = await photoBlob.DeleteIfExistsAsync();
			log.LogInformation(result ? $"Deleted {name}." : $"{name} does not exist.");
			return result;
		}

		[FunctionName("IndexImageMetadata")]
		public static bool IndexImageMetadata([ActivityTrigger] ImageInfo imageInfo, ILogger log)
		{
			string searchServiceName = Environment.GetEnvironmentVariable("SearchServiceName");
			string adminApiKey = Environment.GetEnvironmentVariable("SearchServiceApiKey");

			log.LogInformation($"Indexing {imageInfo.Url}.");

			SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

			var index = serviceClient.Indexes.CreateOrUpdate(new Index
			{
				Name = "photos",
				Fields = FieldBuilder.BuildForType<ImageInfo>()
			});

			SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, "photos", new SearchCredentials(adminApiKey));
			imageInfo.Id = Guid.NewGuid().ToString();
			imageInfo.Uploaded = DateTimeOffset.Now;
			var batch = IndexBatch.Upload(new[] { imageInfo });

			try
			{
				indexClient.Documents.Index(batch);
			}
			catch (IndexBatchException ibEx)
			{
				log.LogError(ibEx, $"Error indexing {imageInfo.Url}");
				throw;
			}

			return true;
		}

		private static async Task<string> GetBlobAccessUrl(string containerName, string name, TimeSpan timeout)
		{
			var imageBlob = await GetImageFromContainer(containerName, name);
			var accessSignature = imageBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy { Permissions = SharedAccessBlobPermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.Add(timeout) });

			return imageBlob.Uri.ToString() + accessSignature;
		}

		private static async Task<CloudBlockBlob> GetImageFromContainer(string containerName, string name)
		{
			var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("UploadStorage"));
			var client = account.CreateCloudBlobClient();
			var container = client.GetContainerReference(containerName);
			var containerExists = await container.ExistsAsync();
			if (!containerExists)
			{
				await container.CreateIfNotExistsAsync();
				BlobContainerPermissions permissions = await container.GetPermissionsAsync();
				permissions.PublicAccess = BlobContainerPublicAccessType.Container;
				await container.SetPermissionsAsync(permissions);
			}

			return container.GetBlockBlobReference(name);
		}
	}
}
