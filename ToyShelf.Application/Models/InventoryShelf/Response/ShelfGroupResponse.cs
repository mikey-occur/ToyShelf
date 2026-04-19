using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.InventoryShelf.Response
{
	public class ShelfGroupResponse
	{
		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = string.Empty;
		public string? ImageUrl { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public double Depth { get; set; }
		public string? DisplayGuideline { get; set; }
		public int TotalLevels { get; set; }

		// Các cột số lượng đã được gom lại
		public int Available { get; set; }
		public int Reserved { get; set; }
		public int InTransit { get; set; }
		public int InUse { get; set; }
		public int Recalled { get; set; }
		public int PendingMaintenance { get; set; }
		public int Maintenance { get; set; }
		public int Retired { get; set; }
	}
}
