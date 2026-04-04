using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Order
{
	public class OrderWithCommissionResponse
	{
		public Guid Id { get; set; }
		public long OrderCode { get; set; }
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerPhone { get; set; } = string.Empty;
		public decimal TotalAmount { get; set; }
		public string PaymentMethod { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public string? StoreName { get; set; }
		public decimal TotalCommission { get; set; }
		public List<OrderItemWithCommissionResponse> Items { get; set; } = new();
	}
}
