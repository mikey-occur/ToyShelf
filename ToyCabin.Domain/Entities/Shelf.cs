using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class Shelf
	{
		public Guid Id { get; set; }
		public Guid CabinId { get; set; }
		public string Code { get; set; } = null!;
		public int Level { get; set; }
		public bool IsActive { get; set; }
		public virtual Cabin Cabin { get; set; } = null!;
		public virtual ICollection<ShelfSlot> ShelfSlots { get; set; } = new List<ShelfSlot>();
	}

}
