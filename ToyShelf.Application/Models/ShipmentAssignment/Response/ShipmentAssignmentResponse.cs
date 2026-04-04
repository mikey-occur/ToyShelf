using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.ShipmentAssignment.Response
{
	public class ShipmentAssignmentResponse
	{
		public Guid Id { get; set; }

		public Guid? StoreOrderId { get; set; }
		public string? StoreOrderCode { get; set; }

		public Guid? ShelfOrderId { get; set; }
		public string? ShelfOrderCode { get; set; }

		// ⭐ thêm để FE xử lý rõ ràng
		public string OrderType { get; set; } = null!; // "STORE" | "SHELF"

		public Guid WarehouseLocationId { get; set; }
		public string WarehouseLocationName { get; set; } = null!;

		public Guid StoreLocationId { get; set; }
		public string StoreLocationName { get; set; } = null!;

		public string? ShipperName { get; set; }

		public string CreatedByName { get; set; } = null!;
		public string? AssignedByName { get; set; }

		public AssignmentStatus Status { get; set; }
		public ShipmentStatus ShipmentStatus { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? RespondedAt { get; set; }

		public List<ShipmentAssignmentProductItemResponse>? ProductItems { get; set; }
		public List<ShipmentAssignmentShelfItemResponse>? ShelfItems { get; set; }
	}

	public class ShipmentAssignmentProductItemResponse
	{
		public Guid ProductColorId { get; set; }
		public string SKU { get; set; } = null!;
		public string ProductName { get; set; } = null!;
		public string Color { get; set; } = null!;
		public string? ImageUrl { get; set; }

		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; }
	}

	public class ShipmentAssignmentShelfItemResponse
	{
		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = null!;
		public string? ImageUrl { get; set; }

		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; }
	}
}
