using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Search;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Kopis.Photos.Functions
{
	public class ImageInfo
	{
		[Key]
		public string Id { get; set; }

		[IsFilterable, IsSearchable]
		public string Url { get; set; }

		[IsFilterable, IsSearchable]
		public string ThumbnailUrl { get; set; }

		[IsFilterable, IsSearchable, IsFacetable]
		public string[] Categories { get; set; }

		[IsFilterable, IsSearchable, IsFacetable]
		public string[] Tags { get; set; }

		[IsFilterable, IsSearchable, IsFacetable]
		public string[] Captions { get; set; }

		[IsFilterable, IsFacetable, IsSortable]
		public int Height { get; set; }

		[IsFilterable, IsFacetable, IsSortable]
		public int Width { get; set; }

		[IsFilterable, IsSearchable, IsFacetable, IsSortable]
		public string Format { get; set; }

		[IsFilterable, IsFacetable, IsSortable]
		public DateTimeOffset? Uploaded { get; set; }

		[IsFilterable, IsSearchable, IsFacetable, IsSortable]
		public string UploadedBy { get; set; }

		public void SetAnalysisFields(ImageAnalysis analysis)
		{
			Categories = analysis.Categories.Select(c => c.Name).ToArray();
			Tags = analysis.Description.Tags.ToArray();
			Captions = analysis.Description.Captions.Select(c => c.Text).ToArray();
			Height = analysis.Metadata.Height;
			Width = analysis.Metadata.Width;
			Format = analysis.Metadata.Format;
		}
	}
}
