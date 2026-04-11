using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Request
{
	public class StoreReceiveRequest
	{
		public List<ReceiveProductItemDetail> ProductItems { get; set; } = new();
		public List<ReceiveShelfItemDetail> ShelfItems { get; set; } = new();
	}

	public class ReceiveProductItemDetail
	{
		public Guid ShipmentItemId { get; set; } // Lấy từ ShipmentProductItemDto.ShipmentItemId
		public int ReceivedQuantity { get; set; } // Lấy từ ô Input nhân viên nhập
	}

	public class ReceiveShelfItemDetail
	{
		public Guid ShelfShipmentItemId { get; set; } // Lấy từ ShipmentShelfItemDto.ShelfShipmentItemId
		public bool IsReceived { get; set; } // Checkbox: Có nhận được kệ hay không
	}
}
