using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class DamageReport
	{
		public Guid Id { get; set; }

		public Guid InventoryLocationId { get; set; }
		public Guid ProductColorId { get; set; }

		public int Quantity { get; set; }
		public string? Description { get; set; }

		public string Status { get; set; } = null!;   // PENDING, APPROVED, REJECTED, COMPENSATED

		public Guid ReportedByUserId { get; set; }
		public DateTime CreatedAt { get; set; }

		public Guid? ReviewedByUserId { get; set; }
		public DateTime? ReviewedAt { get; set; }

		// Navigation
		public virtual InventoryLocation InventoryLocation { get; set; } = null!;
		public virtual ProductColor ProductColor { get; set; } = null!;
		public virtual User ReportedByUser { get; set; } = null!;
		public virtual User? ReviewedByUser { get; set; }
		public virtual ICollection<DamageMedia> DamageMedia { get; set; } = new List<DamageMedia>();
	}

}
