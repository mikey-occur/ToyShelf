using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class ShelfType
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty; 
		public string? Description { get; set; }
		public string? ImageUrl { get; set; }
		public double Width { get; set; }  
		public double Height { get; set; } 
		public double Depth { get; set; }
		// tổng số tầng
		public int TotalLevels { get; set; }
	    // trưng bày tổng thể  và hướng dẫn sử dụng tổng thể của kệ 
		public string SuitableProductCategoryTypes { get; set; } = string.Empty;
		public string? DisplayGuideline { get; set; }

		public bool IsActive { get; set; } = true;

		public virtual ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
		public virtual ICollection<ShelfTypeLevel> ShelfTypeLevels { get; set; } = new List<ShelfTypeLevel>();
		public virtual ICollection<ShelfOrderItem> ShelfOrderItems { get; set; } = new List<ShelfOrderItem>();
		public virtual ICollection<InventoryShelf> InventoryShelves { get; set; } = new List<InventoryShelf>();

	}
}
