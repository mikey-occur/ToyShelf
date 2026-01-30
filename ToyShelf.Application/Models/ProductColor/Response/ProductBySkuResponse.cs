using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ProductColor.Response
{
	public class ProductBySkuResponse
	{
		public Guid ProductId { get; set; }
		public string ProductSku { get; set; } = null!;
		public string ProductName { get; set; } = null!;
		public decimal Price { get; set; }
		public string? Description { get; set; }
		public string? QrCode { get; set; }
		public string? Model3DUrl { get; set; }
		public string? ImageUrl { get; set; }
		public string? Brand { get; set; }
		public string? Material { get; set; }
		public string? OriginCountry { get; set; }
		public string? AgeRange { get; set; }
		public bool IsConsignment { get; set; } = true;
		public string VariantSku { get; set; } = null!;
		public string ColorName { get; set; } = null!;

	}
}
