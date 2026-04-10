using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class ShipmentItem
	{
		public Guid Id { get; set; }
		public Guid ShipmentId { get; set; }

		// Nguồn gốc item (Để khi Receive biết cộng Fulfilled cho Order nào)
		public Guid? StoreOrderItemId { get; set; }
		public Guid? DamageReportItemId { get; set; }

		// Cho phép null khi DamageReportItem là Shelf (Không có ProductColorId)
		public Guid? ProductColorId { get; set; }
		public Guid? ShelfId { get; set; }
		public int ExpectedQuantity { get; set; }
		public int ReceivedQuantity { get; set; }

		public Shipment Shipment { get; set; } = null!;
		public virtual Shelf? Shelf { get; set; }
		public virtual ProductColor? ProductColor { get; set; }
		public virtual StoreOrderItem? StoreOrderItem { get; set; }
		public virtual DamageReportItem? DamageReportItem { get; set; }
	}
}
