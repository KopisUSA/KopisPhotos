using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;

namespace Kopis.Photos.Web.Pages
{
	public class PhotoViewModel
	{
		public string ThumbnailUrl { get; set; }

		public DateTimeOffset UploadedDate { get; set; }

		public string Caption { get; set; }

		public string[] Tags { get; set; }
	}

	public class IndexModel : PageModel
	{
		private readonly IConfiguration _configuration;

		public IList<PhotoViewModel> Results { get; set; }

		public IndexModel(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void OnGet(string query)
		{
			Results = new List<PhotoViewModel>();

			var searchServiceName = _configuration.GetValue<string>("Values:SearchServiceName");
			var searchServiceKey = _configuration.GetValue<string>("Values:SearchServiceApiKey");

			if(string.IsNullOrWhiteSpace(searchServiceName) || string.IsNullOrWhiteSpace(searchServiceKey))
			{
				return;
			}

			SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(searchServiceKey));
			if (serviceClient.Indexes.Exists("photos"))
			{
				SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, "photos", new SearchCredentials(searchServiceKey));

				var searchParams = new SearchParameters()
				{
					Select = new[] { "ThumbnailUrl", "Captions", "Uploaded", "Tags" },
					Top = 10
				};

				if (string.IsNullOrWhiteSpace(query))
				{
					var results = indexClient.Documents.Search("*", searchParams);
					Results = results.Results.Select(r => new PhotoViewModel
					{
						ThumbnailUrl = r.Document["ThumbnailUrl"] as String,
						UploadedDate = DateTimeOffset.Parse(r.Document["Uploaded"].ToString()),
						Caption = ((string[])r.Document["Captions"]).FirstOrDefault(),
						Tags = (string[])r.Document["Tags"]
					}).ToList();
				}
				else
				{
					var results = indexClient.Documents.Search(query, searchParams);
					Results = results.Results.Select(r => new PhotoViewModel
					{
						ThumbnailUrl = r.Document["ThumbnailUrl"] as String,
						UploadedDate = DateTimeOffset.Parse(r.Document["Uploaded"].ToString()),
						Caption = ((string[])r.Document["Captions"]).FirstOrDefault(),
						Tags = (string[])r.Document["Tags"]
					}).ToList();
				}
			}
		}
	}
}
