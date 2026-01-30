using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Order
	{
		public Guid Id { get; set; }
		public Guid StoreId { get; set; }
		public Guid StaffId { get; set; }

		public decimal TotalAmount { get; set; }
		public string PaymentMethod { get; set; } = null!;   // CASH, QR
		public string Status { get; set; } = null!;          // CREATED, PAID, CANCELLED
		public DateTime CreatedAt { get; set; }

		public virtual Store Store { get; set; } = null!;
		public virtual User Staff { get; set; } = null!;

		public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
	}
}
