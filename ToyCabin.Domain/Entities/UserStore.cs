using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public enum StoreRole
	{
		Owner, // -> 1 -> partner_admin
		Manager, // -> partner_manager
		Staff // -> partner_staff
	}
	public class UserStore
	{
		public Guid UserId { get; set; }
		public Guid StoreId { get; set; }
		public StoreRole StoreRole { get; set; } // MANAGER, STAFF
		public bool IsActive { get; set; }
		public virtual User User { get; set; } = null!;
		public virtual Store Store { get; set; } = null!;
	}
}
