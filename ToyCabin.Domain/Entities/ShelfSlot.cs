using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class ShelfSlot
	{
		public Guid Id { get; set; }
		public Guid ShelfId { get; set; }
		public string Code { get; set; } = null!;
		public int DisplayCapacity { get; set; }
		public decimal IdealWeight { get; set; }
		public bool IsActive { get; set; } = true;
		public virtual Shelf Shelf { get; set; } = null!;
	}

}
