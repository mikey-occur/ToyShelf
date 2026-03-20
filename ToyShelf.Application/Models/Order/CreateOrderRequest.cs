using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Order
{
	public class CreateOrderRequest
	{
		// ID của cửa hàng phát sinh đơn hàng
		public Guid StoreId { get; set; }

		public Guid StaffId { get; set; }

		public string CustomerName { get; set; } = string.Empty;
		public string CustomerPhone { get; set; } = string.Empty;

		// Danh sách các món hàng
		public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
	}

	public class OrderItemRequest
	{
		public Guid ProductColorId { get; set; }
		public int Quantity { get; set; }
	}
}
