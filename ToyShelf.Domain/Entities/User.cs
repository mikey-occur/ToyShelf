using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public Guid? PartnerId { get; set; }
		public string Email { get; set; } = string.Empty;	
		public string FullName { get; set; } = string.Empty;
		public string? AvatarUrl { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual Partner? Partner { get; set; }
		public virtual ICollection<UserStore> UserStores { get; set; } = new List<UserStore>();
		public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
		public virtual ICollection<StoreInvitation> StoreInvitations { get; set; } = new List<StoreInvitation>();
		public virtual ICollection<DamageReport> ReportedDamageReports { get; set; } = new List<DamageReport>();
		public virtual ICollection<DamageReport> ReviewedDamageReports { get; set; } = new List<DamageReport>();
		public virtual ICollection<Shipment> RequestedShipments { get; set; } = new List<Shipment>();
		public virtual ICollection<Shipment> ApprovedShipments { get; set; } = new List<Shipment>();
		public virtual ICollection<ShipmentMedia> UploadedShipmentMedia { get; set; } = new List<ShipmentMedia>();
		public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
	}
}
