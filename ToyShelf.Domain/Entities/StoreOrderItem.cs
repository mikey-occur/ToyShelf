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

		// Navigation
		public virtual StoreOrder StoreOrder { get; set; } = null!;
		public virtual ProductColor ProductColor { get; set; } = null!;
	}
}
