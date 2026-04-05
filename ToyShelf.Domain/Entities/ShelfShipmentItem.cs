using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShelfShipmentStatus
	{
		InTransit,
		Received,
		Damaged
	}

	public class ShelfShipmentItem
	{
		public Guid Id { get; set; }

		public Guid ShipmentId { get; set; }
		public Guid ShelfId { get; set; }

		public ShelfShipmentStatus Status { get; set; }

		public virtual Shipment Shipment { get; set; } = null!;
		public virtual Shelf Shelf { get; set; } = null!;
	}
}
