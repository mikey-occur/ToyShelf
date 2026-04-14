using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class ShelfOrderItem
	{
		public Guid Id { get; set; }

		public Guid ShelfOrderId { get; set; }
		public Guid ShelfTypeId { get; set; }

		public int Quantity { get; set; }
		public int FulfilledQuantity { get; set; } = 0;

		public string ShelfTypeName { get; set; } = null!;
		public string? ImageUrl { get; set; }

		public virtual ShelfOrder ShelfOrder { get; set; } = null!;
		public virtual ShelfType ShelfType { get; set; } = null!;
		public virtual ICollection<ShelfShipmentItem> ShelfShipmentItems { get; set; } = new List<ShelfShipmentItem>();
		public virtual ICollection<AssignmentShelfOrderItem> AssignmentShelfOrderItems { get; set; } = new List<AssignmentShelfOrderItem>();
	}
}
