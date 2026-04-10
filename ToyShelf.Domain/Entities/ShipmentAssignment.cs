using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum AssignmentStatus
	{
		Pending,    // Mới tạo (Admin duyệt đơn xong), đang đợi gom đơn hoặc đợi Manager chỉ định.
		Assigned,   // Manager đã chọn Shipper, nhưng Shipper chưa bấm xác nhận trên App.
		Accepted,   // Shipper đã đồng ý nhận nhiệm vụ.
		Rejected,   // Shipper từ chối nhiệm vụ (Manager phải chọn người khác).
		InProgress, // Shipper đã bắt đầu đi thực hiện (đã Pickup kiện hàng đầu tiên).
		Completed,  // Toàn bộ các Shipment con (Giao/Thu hồi) đã hoàn thành (Delivered/Received).
		Cancelled   // Admin hoặc Manager hủy nhiệm vụ này do thay đổi kế hoạch.
	}

	public enum AssignmentType
	{
		Delivery,    // Chỉ giao (StoreOrder/ShelfOrder)
		Return,      // Chỉ thu hồi (DamageReport)
		Combined     // Vừa giao vừa thu hồi
	}

	public class ShipmentAssignment
	{
		public Guid Id { get; set; }

		public Guid WarehouseLocationId { get; set; }

		public Guid? ShipperId { get; set; }

		public AssignmentType Type { get; set; }
		public AssignmentStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? RespondedAt { get; set; }

		public Guid CreatedByUserId { get; set; }
		public Guid? AssignedByUserId { get; set; }

		public virtual User? Shipper { get; set; }
		public virtual User CreatedByUser { get; set; } = null!;
		public virtual User? AssignedByUser { get; set; } 
		public virtual InventoryLocation WarehouseLocation { get; set; } = null!;
		public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
		public virtual ICollection<AssignmentDamageReport> AssignmentDamageReports { get; set; } = new List<AssignmentDamageReport>();
		public virtual ICollection<AssignmentStoreOrder> AssignmentStoreOrders { get; set; } = new List<AssignmentStoreOrder>();
		public virtual ICollection<AssignmentShelfOrder> AssignmentShelfOrders { get; set; } = new List<AssignmentShelfOrder>();
	}
}
