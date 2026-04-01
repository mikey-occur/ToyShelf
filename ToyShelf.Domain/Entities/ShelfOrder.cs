using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum ShelfOrderStatus
	{
		Pending,
		Approved,
		Rejected,
		PartiallyFulfilled,
		Fulfilled
	}
	public class ShelfOrder
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid StoreLocationId { get; set; }
		public Guid RequestedByUserId { get; set; }

		public Guid? ApprovedByUserId { get; set; }
		public Guid? RejectedByUserId { get; set; }

		public ShelfOrderStatus Status { get; set; }

		public string? Note { get; set; }        // store ghi chú
		public string? AdminNote { get; set; }   // admin phản hồi

		public DateTime CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }
		public DateTime? RejectedAt { get; set; }

		// Navigation
		public virtual InventoryLocation StoreLocation { get; set; } = null!;
		public virtual User RequestedByUser { get; set; } = null!;
		public virtual User? ApprovedByUser { get; set; }
		public virtual User? RejectedByUser { get; set; }

		public virtual ICollection<ShelfOrderItem> Items { get; set; } = new List<ShelfOrderItem>();

		public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
		public virtual ICollection<ShipmentAssignment> ShipmentAssignments { get; set; } = new List<ShipmentAssignment>();
	}

}
