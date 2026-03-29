using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Partner
	{
		public Guid Id { get; set; }
		public Guid PartnerTierId { get; set; }
		public string Code { get; set; } = string.Empty; // PARTNER-001
		public string CompanyName { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public PartnerTier PartnerTier { get; set; } = null!;
		public virtual ICollection<User> Users { get; set; } = new List<User>();
		public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
		//public virtual ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
		public virtual ICollection<CommissionTableApply> CommissionTableApplies { get; set; } = new List<CommissionTableApply>();
		public virtual ICollection<CommissionHistory> CommissionHistories { get; set; } = new List<CommissionHistory>();
		public virtual ICollection<StoreCreationRequest> StoreCreationRequests { get; set; } = new List<StoreCreationRequest>();
		public virtual ICollection<MonthlySettlement> MonthlySettlements { get; set; } = new List<MonthlySettlement>();

	}
}
