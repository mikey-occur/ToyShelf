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

		public Guid? ShipmentId { get; set; }

		public Guid StoreLocationId { get; set; }
		public Guid RequestedByUserId { get; set; }

		public Guid? ApprovedByUserId { get; set; }
		public Guid? RejectedByUserId { get; set; }

		public StoreOrderStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public DateTime? RejectedAt { get; set; }

		// Navigation
		public virtual InventoryLocation StoreLocation { get; set; } = null!;
		public virtual User RequestedByUser { get; set; } = null!;
		public virtual User? ApprovedByUser { get; set; }
		public virtual User? RejectedByUser { get; set; }

		public virtual ICollection<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();

		public virtual Shipment? Shipment { get; set; }
		public virtual ICollection<AssignmentStoreOrder> AssignmentStoreOrders { get; set; } = new List<AssignmentStoreOrder>();
	}
}
