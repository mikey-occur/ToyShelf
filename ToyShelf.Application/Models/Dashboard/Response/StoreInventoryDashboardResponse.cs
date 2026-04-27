using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class StoreInventoryDashboardResponse
	{
		public Guid StoreId { get; set; }
		public int TotalShelves { get; set; }
		public int TotalProducts { get; set; }
	}

}
