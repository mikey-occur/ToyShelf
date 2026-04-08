using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Product.Request
{
	public class ExcelProductImport
	{
		public string CategoryCode { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public decimal BasePrice { get; set; }
		public string? Description { get; set; }
		public string? Barcode { get; set; }
		public string? Brand { get; set; }
		public string? Material { get; set; }
		public string? OriginCountry { get; set; }
		public string? AgeRange { get; set; }
		public decimal? Width { get; set; }
		public decimal? Length { get; set; }
		public decimal? Height { get; set; }
		public decimal? Weight { get; set; }
		public string ColorName { get; set; } = string.Empty;
		public decimal ColorPrice { get; set; }
		public string? ImageUrl { get; set; }
		public string? Model3DUrl { get; set; }
	}
}
