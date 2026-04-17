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
		public List<Guid> ShipmentAssignmentIds { get; set; } = new List<Guid>();
		public string StoreName { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;
		public Guid RequestedByUserId { get; set; }
		public string RequestName { get; set; } = string.Empty;
		public Guid? ApprovedByUserId { get; set; }
		public string ApproveName { get; set; } = string.Empty;
		public Guid? RejectedByUserId { get; set; }
		public string RejectName { get; set; } = string.Empty;
		public StoreOrderStatus Status { get; set; }
			
		public DateTime CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public DateTime? RejectedAt { get; set; }

		public List<StoreOrderItemResponse> Items { get; set; } = new();
	}
	public class StoreOrderItemResponse
	{
		public Guid ProductColorId { get; set; }
		public string SKU { get; set; } = string.Empty;
		public string ProductName { get; set; } = null!;
		public string Color { get; set; } = null!;
		public string? ImageUrl { get; set; }
		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; } = 0;
	}
}
