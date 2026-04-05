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
		public List<CreateShipmentItemRequest> Items { get; set; } = new();
	}

	public class CreateShipmentItemRequest
	{
		public Guid? ProductColorId { get; set; }
		public Guid? ShelfTypeId { get; set; }

		public int? ExpectedQuantity { get; set; }

		// Phục vụ Auto
		public List<Guid>? ShelfIds { get; set; }
	}
}
