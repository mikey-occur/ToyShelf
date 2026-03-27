using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Inventory.Response
{
	// Response models
	public class LocationInventoryOverviewResponse
	{
		public Guid LocationId { get; set; }
		public string LocationName { get; set; } = null!;
		public InventoryLocationType Type { get; set; }
		public List<ProductInventoryOverviewItem> Products { get; set; } = new();
	}

	public class ProductInventoryOverviewItem
	{
		public Guid ProductId { get; set; }
		public string ProductSKU { get; set; } = string.Empty;
		public string ProductName { get; set; } = null!;
		public string? Description { get; set; }
		public string? Brand { get; set; }
		public string? Material { get; set; }
		public string? OriginCountry { get; set; }
		public string? AgeRange { get; set; }
		public decimal BasePrice { get; set; }
		public List<ColorInventoryOverviewItem> Colors { get; set; } = new();
	}

	public class ColorInventoryOverviewItem
	{
		public Guid ProductColorId { get; set; }
		public string ProductColorSku { get; set; } = null!;
		public string ColorName { get; set; } = null!;
		public string? Model3DUrl { get; set; }
		public string? ImageUrl { get; set; }
		public decimal ProductColorPrice { get; set; }
		public int Available { get; set; }
		public int InTransit { get; set; }
		public int Damaged { get; set; }
		public int Sold { get; set; }
	}

}
