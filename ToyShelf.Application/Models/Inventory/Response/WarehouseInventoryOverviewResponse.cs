using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Inventory.Response
{
	public class WarehouseInventoryOverviewResponse
	{
		public Guid WarehouseId { get; set; }
		public string WarehouseName { get; set; } = null!;
		public List<ProductInventoryOverviewItem> Products { get; set; } = new();
	}

	public class ProductInventoryOverviewItem
	{
		public Guid ProductId { get; set; }
		public string ProductName { get; set; } = null!;
		public List<ColorInventoryOverviewItem> Colors { get; set; } = new();
	}

	public class ColorInventoryOverviewItem
	{
		public Guid ProductColorId { get; set; }
		public string ColorName { get; set; } = null!;

		public int Available { get; set; }
		public int InTransit { get; set; }
		public int Damaged { get; set; }
		public int Sold { get; set; }
	}
}
