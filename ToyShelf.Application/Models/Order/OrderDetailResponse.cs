using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Order
{
	public class OrderDetailResponse
	{
		public Guid Id { get; set; }
		public Guid StoreId { get; set; }
		public string? StoreName { get; set; }
		public string? PartnerName { get; set; }	
		public Guid StaffId { get; set; }
		public string StaffName { get; set; } = string.Empty;
		public string StaffEmail { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string CustomerEmail { get; set; } = string.Empty;
		public string? BankReference { get; set; }
		public long OrderCode { get; set; }
		public decimal TotalAmount { get; set; }
		public string PaymentMethod { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	
		public List<OrderItemDetailResponse> Items { get; set; } = new List<OrderItemDetailResponse>();
	}


	    public class OrderItemDetailResponse
		{
			public Guid ProductColorId { get; set; }
			public string ProductName { get; set; } = string.Empty;
			public string Sku { get; set; } = string.Empty;
			public string? ImageUrl { get; set; }
			public decimal Price { get; set; }
			public int Quantity { get; set; }
			public decimal SubTotal => Price * Quantity;
		}
}

