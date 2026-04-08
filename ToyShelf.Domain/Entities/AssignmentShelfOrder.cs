using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class AssignmentShelfOrder
	{
		public Guid Id { get; set; }
		public Guid ShelfOrderId { get; set; }
		public Guid ShipmentAssignmentId { get; set; }

		public virtual ShelfOrder ShelfOrder { get; set; } = null!;
		public virtual ShipmentAssignment ShipmentAssignment { get; set; } = null!;
	}
}
