using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class StoreDashboardResponse
	{
		public Guid StoreId { get; set; }
		public int TotalOrders { get; set; }
		public decimal TotalRevenue { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }

	}
}
