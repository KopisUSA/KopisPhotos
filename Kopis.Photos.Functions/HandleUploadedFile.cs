using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace Kopis.Photos.Functions
{
	public static class HandleUploadedFile
    {
        [FunctionName("HandleUploadedFile")]
        public static async Task Run([BlobTrigger("uploaded/{name}", Connection = "UploadStorage")]CloudBlockBlob myBlob, string name, [OrchestrationClient] DurableOrchestrationClient starter, ILogger log)
        {
			if (name.ToLower().EndsWith("jpg"))
			{
				await starter.StartNewAsync("ImageApproval", name);
				log.LogInformation($"Started processing {name} \n");
			}
			else
			{
				log.LogInformation($"{name} is not a photo \n");
				await myBlob.DeleteIfExistsAsync();
			}

			log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n");
		}
    }
}
