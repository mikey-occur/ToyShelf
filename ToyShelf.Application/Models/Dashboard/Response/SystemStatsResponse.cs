using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class SystemStatsResponse
	{
		public int TotalOrders { get; set; }
		public decimal TotalRevenue { get; set; }
	}
}
