using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class AssignmentStoreOrder
	{
		public Guid Id { get; set; }
		public Guid StoreOrderId { get; set; }
		public Guid ShipmentAssignmentId { get; set; }

		public virtual StoreOrder StoreOrder { get; set; } = null!;
		public virtual ShipmentAssignment ShipmentAssignment { get; set; } = null!;
		public virtual ICollection<AssignmentStoreOrderItem> AssignmentStoreOrderItems { get; set; } = new List<AssignmentStoreOrderItem>();
	}
}
