using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.ShelfOrder.Response
{
	public class ShelfOrderResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid StoreLocationId { get; set; }
		public string StoreName { get; set; } = string.Empty;

		public Guid RequestedByUserId { get; set; }
		public string RequestName { get; set; } = string.Empty;

		public Guid? ApprovedByUserId { get; set; }
		public string ApproveName { get; set; } = string.Empty;

		public Guid? RejectedByUserId { get; set; }
		public string RejectName { get; set; } = string.Empty;

		public ShelfOrderStatus Status { get; set; }

		public string? Note { get; set; }
		public string? AdminNote { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public DateTime? RejectedAt { get; set; }

		public List<ShelfOrderItemResponse> Items { get; set; } = new();
	}

	public class ShelfOrderItemResponse
	{
		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }

		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; }
	}

}
