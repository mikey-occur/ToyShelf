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
		Fulfilled
	}

	public class StoreOrder
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;

		public Guid StoreLocationId { get; set; }
		public Guid RequestedByUserId { get; set; }

		public StoreOrderStatus Status { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? ApprovedAt { get; set; }

		// Navigation
		public virtual InventoryLocation StoreLocation { get; set; } = null!;
		public virtual User RequestedByUser { get; set; } = null!;

		public virtual ICollection<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();
	}
}
