using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class ProductColor
	{
		public Guid Id { get; set; }
		public Guid ProductId { get; set; }
		public string Sku { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string HexCode { get; set; } = null!;
		public bool IsActive { get; set; }
		public virtual Product Product { get; set; } = null!;
		public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
		public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
		public virtual ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
		public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
		public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();
	}
}
