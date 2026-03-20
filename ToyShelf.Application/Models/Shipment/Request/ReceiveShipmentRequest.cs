using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Request
{
	public class ReceiveShipmentRequest
	{
		public List<ReceiveShipmentItemRequest> Items { get; set; }
	}

	public class ReceiveShipmentItemRequest
	{
		public Guid ProductColorId { get; set; }
		public int ReceivedQuantity { get; set; }
	}
}
