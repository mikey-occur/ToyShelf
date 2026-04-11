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

		public List<OrderReferenceResponse> StoreOrders { get; set; } = new();
		public List<OrderReferenceResponse> ShelfOrders { get; set; } = new();
		public List<OrderReferenceResponse> DamageReports { get; set; } = new();

		public string? AdminNote { get; set; }

		// ⭐ thêm để FE xử lý rõ ràng
		public string OrderType { get; set; } = null!; // "STORE" | "SHELF"

		public Guid WarehouseLocationId { get; set; }
		public string WarehouseLocationName { get; set; } = null!;

		public Guid StoreLocationId { get; set; }
		public string StoreLocationName { get; set; } = null!;

		public string? ShipperName { get; set; }

		public string CreatedByName { get; set; } = null!;
		public string? AssignedByName { get; set; }

		public AssignmentType Type { get; set; }
		public AssignmentStatus Status { get; set; }
		public ShipmentStatus ShipmentStatus { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? RespondedAt { get; set; }

		public List<ShipmentAssignmentProductItemResponse>? ProductItems { get; set; }
		public List<ShipmentAssignmentShelfItemResponse>? ShelfItems { get; set; }
		public List<ShipmentAssignmentDamageItemResponse>? DamageReturnItems { get; set; } = new();
	}

	public class ShipmentAssignmentProductItemResponse
	{
		public Guid StoreOrderId { get; set; }
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

		public double Width { get; set; }
		public double Height { get; set; }
		public double Depth { get; set; }
		public int TotalLevels { get; set; }

		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; }
	}

	public class ShipmentAssignmentDamageItemResponse
	{
		public Guid DamageReportId { get; set; }
		public string DamageCode { get; set; } = null!;
		public string DamageType { get; set; } = null!; // Product | Shelf
		public string Source { get; set; } = null!;     // Manufacturer | StoreHandling...
		public int Quantity { get; set; }

		public string? TargetName { get; set; } // Tên Sản phẩm hoặc Mã Kệ cụ thể
		public string? Description { get; set; } // Mô tả lỗi từ Store
		public string? ImageUrl { get; set; }    // Ảnh sản phẩm hoặc ảnh hiện trường hỏng
	}
}
