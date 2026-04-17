using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum StoreOrderStatus
	{
		Pending,
		PartnerApproved,
		Approved,
		Rejected,
		Processing,
		PartiallyFulfilled,
		Fulfilled
	}

	public class StoreOrder
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid StoreLocationId { get; set; }
		public Guid RequestedByUserId { get; set; }

		public Guid? PartnerAdminApprovedByUserId { get; set; }
		public Guid? ApprovedByUserId { get; set; }
		public Guid? RejectedByUserId { get; set; }

		public StoreOrderStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? PartnerAdminApprovedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public DateTime? RejectedAt { get; set; }

		// Navigation
		public virtual InventoryLocation StoreLocation { get; set; } = null!;
		public virtual User RequestedByUser { get; set; } = null!;
		public virtual User? PartnerAdminApprovedByUser { get; set; }
		public virtual User? ApprovedByUser { get; set; }
		public virtual User? RejectedByUser { get; set; }

		public virtual ICollection<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();

		public virtual ICollection<AssignmentStoreOrder> AssignmentStoreOrders { get; set; } = new List<AssignmentStoreOrder>();
	}
}
