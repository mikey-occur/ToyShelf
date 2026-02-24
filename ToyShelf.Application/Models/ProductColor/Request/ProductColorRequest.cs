using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ProductColor.Request
{
	public class ProductColorRequest
	{
		public Guid ProductId { get; set; }
		public string Name { get; set; } = null!;
		public Guid ColorId { get; set; }
		public Guid PriceSegmentId { get; set; }
		public decimal Price { get; set; }
		//public string? QrCode { get; set; }
		public string? Model3DUrl { get; set; }
		public string? ImageUrl { get; set; }
	}
}
