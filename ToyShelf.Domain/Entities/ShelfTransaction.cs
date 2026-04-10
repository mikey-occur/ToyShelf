using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShelfReferenceType
	{
		Shipment,
		Recall,
		Maintenance,
		Audit,
		DamageReport
	}

	public class ShelfTransaction
	{
		public Guid Id { get; set; }

		public Guid ShelfId { get; set; }

		public Guid FromLocationId { get; set; }
		public Guid ToLocationId { get; set; }

		public ShelfStatus FromStatus { get; set; }
		public ShelfStatus ToStatus { get; set; }

		public ShelfReferenceType ReferenceType { get; set; }
		public Guid? ReferenceId { get; set; }

		public DateTime CreatedAt { get; set; }

		public virtual Shelf Shelf { get; set; } = null!;
		public virtual InventoryLocation FromLocation { get; set; } = null!;
		public virtual InventoryLocation ToLocation { get; set; } = null!;
	}
}
