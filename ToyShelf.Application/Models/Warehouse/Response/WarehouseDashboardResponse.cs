using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Warehouse.Response
{
	public class WarehouseDashboardResponse
	{
		public int TotalOrders { get; set; }
		public int TotalShelves { get; set; }
		public int TotalInventory { get; set; }
		public int TotalEmployees { get; set; }

		public int TotalInProgressShipments { get; set; }
		public int TotalCompletedShipments { get; set; }

		public List<ChartItem> ShipmentChart { get; set; } = new();
		public List<ChartItem> OrderChart { get; set; } = new();
	}

	public class ChartItem
	{
		public string Status { get; set; } = null!;
		public int Count { get; set; }
	}
}
