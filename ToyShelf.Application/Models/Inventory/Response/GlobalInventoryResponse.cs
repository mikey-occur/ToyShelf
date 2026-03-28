using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Inventory.Response
{
	public class GlobalInventoryResponse
	{
		public Guid Id { get; set; } // LocationId
		public Guid InventoryLocationId { get; set; } 
		public string Name { get; set; } = null!; // LocationName
		public InventoryLocationType Type { get; set; }
		public List<GlobalProductInventoryItem> Products { get; set; } = new();

		public int? PageNumber { get; set; }
		public int? PageSize { get; set; }
		public int TotalCount { get; set; }
	}

	public class GlobalProductInventoryItem
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

		public List<GlobalColorInventoryItem> Colors { get; set; } = new();
	}

	public class GlobalColorInventoryItem
	{
		public Guid ProductColorId { get; set; }
		public Guid ColorId { get; set; }
		public string ProductColorSku { get; set; } = null!;
		public string ColorName { get; set; } = null!;
		public string HexCode { get; set; } = null!;
		public string? Model3DUrl { get; set; }
		public string? ImageUrl { get; set; }
		public decimal ProductColorPrice { get; set; }
		public int Available { get; set; }
		public int InTransit { get; set; }
		public int Damaged { get; set; }
		public int Sold { get; set; }
	}
}
