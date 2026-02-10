using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ProductColor.Response
{
	public class ProductColorResponse
	{
		public Guid Id { get; set; }
		public Guid ProductId { get; set; }
		public string Sku { get; set; } = null!;
		public Guid PriceSegmentId { get; set; }
		public Guid ColorId { get; set; }
		public decimal Price { get; set; }
		public string? QrCode { get; set; }
		public string? Model3DUrl { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsActive { get; set; }
	}
}
