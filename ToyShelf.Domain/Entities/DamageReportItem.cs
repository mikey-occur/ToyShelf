using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum DamageItemType
	{
		Product,
		Shelf
	}
	public class DamageReportItem
	{
		public Guid Id { get; set; }
		public Guid DamageReportId { get; set; }
		public DamageItemType DamageItemType { get; set; } // Product | Shelf

		// Nếu là Product: Cần ID và Số lượng
		public Guid? ProductColorId { get; set; }
		public int? Quantity { get; set; }

		// Nếu là Shelf: CHỈ CẦN ID (Không có số lượng)
		public Guid? ShelfId { get; set; }

		public virtual DamageReport DamageReport { get; set; } = null!;
		public virtual ProductColor? ProductColor { get; set; }
		public virtual Shelf? Shelf { get; set; }
		public virtual ICollection<DamageMedia> DamageMedia { get; set; } = new List<DamageMedia>();
	}
}
