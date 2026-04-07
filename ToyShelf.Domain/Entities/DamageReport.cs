using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum DamageType
	{
		Product,     
		Shelf          
	}

	public enum DamageSource
	{
		Manufacturer,    // Lỗi từ NSX (Sản phẩm lỗi hoặc Kệ lỗi phần cứng)
		StoreHandling,   // Store làm hỏng (Va quệt, làm rơi kệ/hàng)
		CustomerUsage,   // Khách hàng làm hỏng trong quá trình tương tác/chơi thử
		Transportation,  // Hỏng trong quá trình vận chuyển (Logistics)
		IoTSystemError   // Lỗi hệ thống IoT (Ví dụ: Chập điện, lỗi cảm biến đĩa xoay)
	}

	public enum DamageStatus
	{
		Pending,        // Chờ Admin duyệt
		Approved,       // Đã duyệt, chờ tạo/gán vận đơn thu hồi
		InTransit,      // Shipper đang chở hàng/kệ về kho
		Returned,       // Đã về đến kho tổng (Kết thúc quy trình tại Store)
		Rejected        // Từ chối báo cáo (Ví dụ: Store báo hỏng không trung thực)
	}
	public class DamageReport
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;


		// 1. PHÂN LOẠI & NGUỒN GỐC
		public DamageType Type { get; set; }      // Hàng hay Kệ?
		public DamageSource Source { get; set; }  // Lỗi do ai/đâu?
		public DamageStatus Status { get; set; }


		// 2. ĐỐI TƯỢNG BỊ HƯ HẠI
		public Guid? ProductColorId { get; set; }
		public Guid? ShelfId { get; set; }
		public int Quantity { get; set; }


		// 3. THÔNG TIN CHI TIẾT
		public string? Description { get; set; }
		public string? AdminNote { get; set; }


		// 4. ĐỊA ĐIỂM & NHÂN SỰ
		public Guid InventoryLocationId { get; set; } // Store nơi xảy ra sự cố
		public Guid ReportedByUserId { get; set; }
		public DateTime CreatedAt { get; set; }

		public Guid? ReviewedByUserId { get; set; }
		public DateTime? ReviewedAt { get; set; }


		// 6. Bảo hành 
		public bool IsWarrantyClaim { get; set; } = false; // Đánh dấu đây là ca bảo hành

		// Thời hạn bảo hành (nếu cần đối soát nhanh)
		public DateTime? WarrantyExpirationDate { get; set; }

		// Navigation
		public virtual InventoryLocation InventoryLocation { get; set; } = null!;
		public virtual ProductColor? ProductColor { get; set; }
		public virtual Shelf? Shelf { get; set; }
		public virtual Shipment? Shipment { get; set; }
		public virtual User ReportedByUser { get; set; } = null!;
		public virtual User? ReviewedByUser { get; set; }
		public virtual ICollection<DamageMedia> DamageMedia { get; set; } = new List<DamageMedia>();
		public virtual ICollection<ShipmentAssignment> ShipmentAssignments { get; set; } = new List<ShipmentAssignment>();
	}

}
