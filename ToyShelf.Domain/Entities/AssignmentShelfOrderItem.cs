using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class AssignmentShelfOrderItem
	{
		public Guid Id { get; set; }
		public Guid AssignmentShelfOrderId { get; set; }
		public Guid ShelfOrderItemId { get; set; }

		public int AllocatedQuantity { get; set; }

		public virtual AssignmentShelfOrder AssignmentShelfOrder { get; set; } = null!;
		public virtual ShelfOrderItem ShelfOrderItem { get; set; } = null!;
	}
}
