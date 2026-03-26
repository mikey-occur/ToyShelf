using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Warehouse.Response
{
	public class WarehouseMatchResponse
	{
		public Guid WarehouseId { get; set; }
		public string WarehouseName { get; set; } = string.Empty;
		public string WarehouseCode { get; set; } = string.Empty;

		public List<WarehouseItemResponse> Items { get; set; } = new();
	}

	public class WarehouseItemResponse
	{
		public Guid ProductColorId { get; set; }
		public string SKU { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string Color { get; set; } = string.Empty;

		public int AvailableQuantity { get; set; }
		//public int RequestedQuantity { get; set; }
	}
}
