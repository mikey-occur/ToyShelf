using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Product.Response
{
	public class TopSellingProductResponse
	{
		public Guid ProductId { get; set; }
		public Guid ProductColorId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string Sku { get; set; } = string.Empty;
		public string? Brand { get; set; }
		public string ColorName { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public decimal Price { get; set; }
		public int TotalSold { get; set; }
	}
}
