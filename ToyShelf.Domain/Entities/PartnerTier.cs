using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class PartnerTier
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public int Priority { get; set; }
		public int? MaxShelvesPerStore { get; set; }
		public virtual ICollection<Partner> Partners { get; set; } = new List<Partner>();

		public virtual ICollection<CommissionTable> CommissionTables { get; set; }
			= new List<CommissionTable>();

	}
}
