using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class OrderItem
	{
		public Guid Id { get; set; }
		public Guid OrderId { get; set; }
		public Guid ProductColorId { get; set; }

		public int Quantity { get; set; }
		public decimal Price { get; set; }

		public virtual Order Order { get; set; } = null!;
		public virtual ProductColor ProductColor { get; set; } = null!;
		public virtual ICollection<CommissionHistory> CommissionHistories { get; set; } = new List<CommissionHistory>();
	}
}
