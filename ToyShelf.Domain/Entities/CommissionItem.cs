using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class CommissionItem
	{
		public Guid Id { get; set; }
		public Guid CommissionTableId { get; set; }
		public decimal CommissionRate { get; set; } // VD: 0.15 (15%)
		public virtual CommissionTable CommissionTable { get; set; } = null!;
		public virtual ICollection<CommissionItemCategory> ItemCategories { get; set; } = new List<CommissionItemCategory>();

	}
}
