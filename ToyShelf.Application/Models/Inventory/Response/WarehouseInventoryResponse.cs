using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Inventory.Response
{
	public class WarehouseInventoryResponse
	{
		public Guid WarehouseId { get; set; }
		public string WarehouseName { get; set; } = null!;
		public List<ProductInventoryItem> Products { get; set; } = new();
	}

	public class ProductInventoryItem
	{
		public Guid ProductId { get; set; }
		public string ProductName { get; set; } = null!;
		public List<ColorInventoryItem> Colors { get; set; } = new();
	}

	public class ColorInventoryItem
	{
		public Guid ProductColorId { get; set; }
		public string ColorName { get; set; } = null!;
		public int Quantity { get; set; }
	}
}
