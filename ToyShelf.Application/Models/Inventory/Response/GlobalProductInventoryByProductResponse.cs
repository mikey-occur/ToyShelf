using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Inventory.Response
{
	public class GlobalProductInventoryByProductResponse
	{
		public Guid ProductId { get; set; }
		public string ProductName { get; set; } = null!;
		public List<LocationInventoryItem> Locations { get; set; } = new();
	}

	public class LocationInventoryItem
	{
		public Guid LocationId { get; set; }
		public string LocationName { get; set; } = null!;
		public string Type { get; set; } = null!;

		// Danh sách tồn kho theo màu
		public List<ProductColorInventoryItem> Colors { get; set; } = new();
	}

	public class ProductColorInventoryItem
	{
		public Guid ColorId { get; set; }
		public string ColorName { get; set; } = null!;
		public int Available { get; set; }
		public int InTransit { get; set; }
		public int Damaged { get; set; }
		public int Sold { get; set; }
	}

}
