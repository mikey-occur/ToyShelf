using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShipmentStatus
	{
		Draft,
		//Approved,
		Shipping,
		Delivered,
		Received,
		Cancelled
	}

	public class Shipment
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid StoreOrderId { get; set; }
		public Guid? ShipperId { get; set; }

		public Guid FromLocationId { get; set; }
		public Guid ToLocationId { get; set; }

		public Guid RequestedByUserId { get; set; }
		public Guid ShipmentAssignmentId { get; set; }

		public ShipmentStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? PickedUpAt { get; set; }
		public DateTime? DeliveredAt { get; set; }
		public DateTime? ReceivedAt { get; set; }

		public virtual StoreOrder StoreOrder { get; set; } = null!;
		public virtual User? Shipper { get; set; }

		public virtual InventoryLocation FromLocation { get; set; } = null!;
		public virtual InventoryLocation ToLocation { get; set; } = null!;

		public virtual User RequestedByUser { get; set; } = null!;
		public virtual ShipmentAssignment ShipmentAssignment { get; set; } = null!;


		public virtual ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
		public virtual ICollection<ShipmentMedia> Media { get; set; } = new List<ShipmentMedia>();
	}
}
