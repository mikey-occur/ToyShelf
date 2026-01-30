using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum StoreRole
	{
		Staff, // -> partner_staff
		Manager // -> partner_manager
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
