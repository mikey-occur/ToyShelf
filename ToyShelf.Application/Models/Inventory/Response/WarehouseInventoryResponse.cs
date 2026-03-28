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

		public int? PageNumber { get; set; }
		public int? PageSize { get; set; }
		public int TotalCount { get; set; }
	}

	public class ProductInventoryItem
	{
		public Guid ProductId { get; set; }
		public string ProductSKU { get; set; } = string.Empty;
		public string ProductName { get; set; } = null!;

		public Guid ProductCategoryId { get; set; }
		public string ProductCategoryName { get; set; } = string.Empty;

		public string? Description { get; set; }
		public string? Brand { get; set; }
		public string? Material { get; set; }
		public string? OriginCountry { get; set; }
		public string? AgeRange { get; set; }
		public decimal BasePrice { get; set; }
		public List<ColorInventoryItem> Colors { get; set; } = new();
	}

	public class ColorInventoryItem
	{
		public Guid ProductColorId { get; set; }
		public string ProductColorSku { get; set; } = null!;
		public string ColorName { get; set; } = null!;
		public string HexCode { get; set; } = null!;
		public string? Model3DUrl { get; set; }
		public string? ImageUrl { get; set; }
		public decimal ProductColorPrice { get; set; }
		public int Quantity { get; set; }
	}
}
