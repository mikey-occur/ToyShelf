using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShipmentResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid StoreOrderId { get; set; }
		public Guid ToLocationId { get; set; }
		public string ToLocationName { get; set; } = null!;
		public Guid FromLocationId { get; set; }
		public string FromLocationName { get; set; } = null!;

		public string? ShipperName { get; set; }

		public ShipmentStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? PickedUpAt { get; set; }
		public DateTime? DeliveredAt { get; set; }
		public DateTime? ReceivedAt { get; set; }

		public List<ShipmentItemResponse> Items { get; set; } = new();
	}
}
