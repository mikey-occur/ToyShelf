using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class InventoryTransaction
	{
		public Guid Id { get; set; }

		public Guid ProductColorId { get; set; }

		public Guid FromLocationId { get; set; }
		public Guid ToLocationId { get; set; }

		public Guid FromDispositionId { get; set; }
		public Guid ToDispositionId { get; set; }

		public int Quantity { get; set; }

		public string ReferenceType { get; set; } = null!;   // REFILL, SALE, DAMAGE, RECALL, AUDIT
		public Guid? ReferenceId { get; set; }

		public DateTime CreatedAt { get; set; }

		// Navigation
		public virtual ProductColor ProductColor { get; set; } = null!;

		public virtual InventoryLocation FromLocation { get; set; } = null!;
		public virtual InventoryLocation ToLocation { get; set; } = null!;

		public virtual InventoryDisposition FromDisposition { get; set; } = null!;
		public virtual InventoryDisposition ToDisposition { get; set; } = null!;
	}

}
