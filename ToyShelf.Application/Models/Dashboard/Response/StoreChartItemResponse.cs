using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class StoreChartItemResponse
	{
		public string DateLabel { get; set; } = string.Empty; 
		public int TotalOrders { get; set; }                
		public decimal TotalRevenue { get; set; }
	}
}
