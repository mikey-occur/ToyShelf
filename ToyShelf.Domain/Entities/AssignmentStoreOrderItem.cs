using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class AssignmentStoreOrderItem
	{
		public Guid Id { get; set; }
		public Guid AssignmentStoreOrderId { get; set; }
		public Guid StoreOrderItemId { get; set; } 

		public int AllocatedQuantity { get; set; } 

		public virtual AssignmentStoreOrder AssignmentStoreOrder { get; set; } = null!;
		public virtual StoreOrderItem StoreOrderItem { get; set; } = null!;
	}
}
