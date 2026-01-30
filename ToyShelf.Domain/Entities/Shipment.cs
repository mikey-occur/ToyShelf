using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Shipment
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid FromLocationId { get; set; }
		public Guid ToLocationId { get; set; }

		public Guid RequestedByUserId { get; set; }
		public Guid? ApprovedByUserId { get; set; }

		public string Status { get; set; } = null!;

		public virtual InventoryLocation FromLocation { get; set; } = null!;
		public virtual InventoryLocation ToLocation { get; set; } = null!;

		public virtual User RequestedByUser { get; set; } = null!;
		public virtual User ApprovedByUser { get; set; } = null!;

		public virtual ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
		public virtual ICollection<ShipmentMedia> Media { get; set; } = new List<ShipmentMedia>();
	}
}
