using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Order
{
	public class OrderResponse
	{
		public Guid Id { get; set; }
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerEmail { get; set; } = string.Empty;
		public long OrderCode { get; set; }
		public string? BankReference { get; set; }
		public decimal TotalAmount { get; set; }
		public string PaymentMethod { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public string? StoreName { get; set; }
        public bool IsLocked { get; set; } = false;
    }
}
