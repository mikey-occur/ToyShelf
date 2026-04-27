using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Warehouse.Response;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class WarehouseChartResponse
	{
		public List<ChartItem> ShipmentChart { get; set; } = new();
		public List<ChartItem> OrderChart { get; set; } = new();
	}
}
