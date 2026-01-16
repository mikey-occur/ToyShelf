using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class Inventory
	{
		public Guid Id { get; set; }
		public Guid InventoryLocationId { get; set; }
		public Guid ProductColorId { get; set; }
		public Guid DispositionId { get; set; }
		public int Quantity { get; set; }

		public virtual InventoryLocation InventoryLocation { get; set; } = null!; 
		public virtual ProductColor ProductColor { get; set; } = null!;
		public virtual InventoryDisposition Disposition { get; set; } = null!;
	}

}
