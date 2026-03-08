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

		// ID của nhân viên đứng máy tạo đơn (Bắt buộc theo Entity Order của bạn)
		public Guid StaffId { get; set; }

	
		// Danh sách các món hàng
		public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
	}

	public class OrderItemRequest
	{
		public Guid ProductColorId { get; set; }
		public int Quantity { get; set; }
	}
}
