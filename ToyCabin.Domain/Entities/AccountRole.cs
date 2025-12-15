using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class AccountRole
	{
		public Guid AccountId { get; set; }
		public Guid RoleId { get; set; }

		public virtual Account Account { get; set; } = null!;
		public virtual Role Role { get; set; } = null!;

	}
}
