using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Store
	{
		public Guid Id { get; set; }
		public Guid PartnerId { get; set; }
		public string Code { get; set; } = string.Empty; // STORE-HCM01
		public string Name { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual Partner Partner { get; set; } = null!;
		public virtual ICollection<Cabin> Cabins { get; set; } = new List<Cabin>();
		public virtual ICollection<UserStore> UserStores { get; set; } = new List<UserStore>();
		public virtual ICollection<StoreInvitation> StoreInvitations { get; set; }= new List<StoreInvitation>();
		public ICollection<Order> Orders { get; set; } = new List<Order>();
		public virtual ICollection<InventoryLocation> InventoryLocations { get; set; } = new List<InventoryLocation>();
	}
}
