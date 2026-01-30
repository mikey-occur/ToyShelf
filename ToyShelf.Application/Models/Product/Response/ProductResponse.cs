using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.ProductColor.Response;

namespace ToyShelf.Application.Models.Product.Response
{
	public class ProductResponse
	{
		public Guid Id { get; set; }
		public Guid ProductCategoryId { get; set; }
		public string SKU { get; set; } = string.Empty; 
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public string? Description { get; set; }
		public string? Brand { get; set; }
		public string? Material { get; set; }
		public string? OriginCountry { get; set; }
		public string? AgeRange { get; set; }
		public bool IsActive { get; set; } = true;
		public bool IsConsignment { get; set; } = true; 
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public List<ProductColorResponse> Colors { get; set; } = new();
	}
}
