using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.StoreOrder.Response
{
	public class StoreOrderResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;
		public Guid StoreLocationId { get; set; }
		public Guid RequestedByUserId { get; set; }
		public Guid? ApprovedByUserId { get; set; }
		public Guid? RejectedByUserId { get; set; }
		public StoreOrderStatus Status { get; set; }
			
		public DateTime CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public DateTime? RejectedAt { get; set; }

		public List<StoreOrderItemResponse> Items { get; set; } = new();
	}
	public class StoreOrderItemResponse
	{
		public Guid ProductColorId { get; set; }
		public string ProductName { get; set; } = null!;
		public string Color { get; set; } = null!;
		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; } = 0;
	}
}
