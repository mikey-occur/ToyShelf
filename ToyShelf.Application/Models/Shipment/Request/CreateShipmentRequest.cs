using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Request
{
	public class CreateShipmentRequest
	{
		public Guid ShipmentAssignmentId { get; set; }
		// Items chỉ dành cho StoreOrder và ShelfOrder 
		// vì đây là những món WM cần xác nhận số lượng bốc từ kho ra
		public List<CreateShipmentItemRequest> Items { get; set; } = new();
	}

	public class CreateShipmentItemRequest
	{
		public Guid? StoreOrderId { get; set; }
		public Guid? ShelfOrderId { get; set; }

		public Guid? ProductColorId { get; set; }
		public Guid? ShelfTypeId { get; set; }
		public int ExpectedQuantity { get; set; } // Số lượng thực tế WM bốc lên xe
		public List<Guid>? ShelfIds { get; set; } // Dùng cho chọn kệ đích danh
	}
}
