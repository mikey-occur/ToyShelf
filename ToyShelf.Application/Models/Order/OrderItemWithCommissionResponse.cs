using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Order
{
	public class OrderItemWithCommissionResponse
	{
		public Guid ProductColorId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string Sku { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public decimal CommissionAmount { get; set; }
		public decimal AppliedRate { get; set; }
	}
}
