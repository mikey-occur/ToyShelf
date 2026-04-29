using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.DamageReport.Response;
using ToyShelf.Application.Models.ShipmentAssignment.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShipmentResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		// Thay vì 1 ID lẻ, ta trả về danh sách để FE biết chuyến này gom những đơn nào
		public List<OrderReferenceResponse> StoreOrders { get; set; } = new();
		public List<OrderReferenceResponse> ShelfOrders { get; set; } = new();
		public List<OrderReferenceResponse> DamageReports { get; set; } = new();


		// Flag quan trọng để Shipper biết là đi giao hay đi thu hồi
		public bool IsReturn { get; set; }
		public string OrderType { get; set; } = null!; // "STORE", "SHELF", "RETURN", hoặc "MIXED"

		public Guid FromLocationId { get; set; }
		public string FromLocationName { get; set; } = null!;

		public Guid ToLocationId { get; set; }
		public string ToLocationName { get; set; } = null!;

		public string? ShipperName { get; set; }
		public ShipmentStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? PickedUpAt { get; set; }
		public DateTime? DeliveredAt { get; set; }
		public DateTime? StoreReceivedAt { get; set; }
		public DateTime? ReturnPickedUpAt { get; set; }
		public DateTime? ArrivedWarehouseAt { get; set; }
		public DateTime? WarehouseReceivedAt { get; set; }

		// Danh sách sản phẩm tổng hợp (đã gộp từ tất cả StoreOrders hoặc DamageReports)
		public List<ShipmentProductItemResponse>? ProductItems { get; set; }

		// Danh sách kệ tổng hợp (đã GroupBy theo ShelfType)
		public List<ShipmentShelfItemResponse>? ShelfItems { get; set; }

		public List<DamageReturnItemResponse>? DamageReturnItems { get; set; }

		// Thêm thông tin Media (Bằng chứng lúc Pickup/Delivery) nếu FE cần hiển thị
		public List<string>? MediaUrls { get; set; }
	}
}
