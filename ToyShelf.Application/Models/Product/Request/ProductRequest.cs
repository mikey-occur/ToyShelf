using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.ProductColor.Request;

namespace ToyShelf.Application.Models.Product.Request
{
	public class ProductRequest
	{
		public Guid ProductCategoryId { get; set; }
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public string? Description { get; set; }
		public string? Brand { get; set; }
		public string? Material { get; set; }
		public string? OriginCountry { get; set; }
		public string? AgeRange { get; set; }

		public List<ProductColorCreateRequest>? Colors { get; set; }
	}
}
