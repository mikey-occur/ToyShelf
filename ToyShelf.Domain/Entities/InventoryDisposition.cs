using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class InventoryDisposition
	{
		public Guid Id { get; set; }

		public string Code { get; set; } = null!;   // AVAILABLE, IN_TRANSIT, DAMAGED, SOLD...
		public string? Description { get; set; }

		// Navigation
		public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
		public virtual ICollection<InventoryTransaction> FromInventoryTransactions { get; set; } = new List<InventoryTransaction>();
		public virtual ICollection<InventoryTransaction> ToInventoryTransactions { get; set; } = new List<InventoryTransaction>();
	}

}
