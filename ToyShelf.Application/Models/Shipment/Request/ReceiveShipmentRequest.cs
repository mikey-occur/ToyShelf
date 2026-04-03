using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Request
{
	public class ReceiveShipmentRequest
	{
		public List<ReceiveShipmentProductItemRequest>? ProductItems { get; set; }
		public List<ReceiveShipmentShelfItemRequest>? ShelfItems { get; set; }
	}


	public class ReceiveShipmentProductItemRequest
	{
		public Guid ProductColorId { get; set; }
		public int ReceivedQuantity { get; set; }
	}

	public class ReceiveShipmentShelfItemRequest
	{
		public Guid ShelfId { get; set; }
		public bool IsReceived { get; set; } 
	}
}
