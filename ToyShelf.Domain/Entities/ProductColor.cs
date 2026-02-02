using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class ProductColor
	{
		public Guid Id { get; set; }
		public Guid ColorId { get; set; }
		public Guid ProductId { get; set; }
		public Guid PriceSegmentId { get; set; } // để biết product phân khúc giá nào

		public string Sku { get; set; } = null!;
		public decimal Price { get; set; }

		public string? QrCode { get; set; }
		public string? Model3DUrl { get; set; } // link to 3D model
		public string? ImageUrl { get; set; }

		public bool IsActive { get; set; }

		public virtual Color Color { get; set; } = null!;
		public virtual Product Product { get; set; } = null!;
		public virtual PriceSegment PriceSegment { get; set; } = null!;
		public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
		public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
		public virtual ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
		public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
		public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();
	}
}
