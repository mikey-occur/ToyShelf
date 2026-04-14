using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class StoreOrderItem
	{
		public Guid Id { get; set; }

		public Guid StoreOrderId { get; set; }
		public Guid ProductColorId { get; set; }

		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; } = 0;

		// Navigation
		public virtual StoreOrder StoreOrder { get; set; } = null!;
		public virtual ProductColor ProductColor { get; set; } = null!;
		public virtual ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
		public virtual ICollection<AssignmentStoreOrderItem> AssignmentStoreOrderItems { get; set; } = new List<AssignmentStoreOrderItem>();
	}
}
