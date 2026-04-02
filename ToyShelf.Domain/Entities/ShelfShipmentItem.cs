using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class ShelfShipmentItem
	{
		public Guid Id { get; set; }

		public Guid ShipmentId { get; set; }
		public Guid ShelfTypeId { get; set; }

		public int ExpectedQuantity { get; set; }
		public int ReceivedQuantity { get; set; }

		public virtual Shipment Shipment { get; set; } = null!;
		public virtual ShelfType ShelfType { get; set; } = null!;
	}
}
