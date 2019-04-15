using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Kopis.Photos.Web.Pages
{
    public class UploadModel : PageModel
    {
		private readonly IConfiguration _configuration;

		public UploadModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void OnGet()
        {

        }

		public async Task OnPostAsync()
		{
			var storageConnectionString = _configuration.GetValue<string>("Values:StorageConnectionString");
			CloudStorageAccount storageAccount;

			if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
			{
				CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

				var cloudBlobContainer = cloudBlobClient.GetContainerReference("uploaded");
				await cloudBlobContainer.CreateIfNotExistsAsync();

				foreach (var file in Request.Form.Files)
				{
					CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(Guid.NewGuid().ToString() + "_" + file.FileName);
					await cloudBlockBlob.UploadFromStreamAsync(file.OpenReadStream());
				}
			}
			else
			{
				throw new InvalidOperationException("Invalid storage connection string");
			}
		}
	}
}