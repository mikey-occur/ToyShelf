using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Product.Request
{
	public class ProductUpdateRequest
	{
		public Guid ProductCategoryId { get; set; }
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public string? Description { get; set; }
		public string? QrCode { get; set; }
		public string? Model3DUrl { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsConsignment { get; set; } 

	}
}
