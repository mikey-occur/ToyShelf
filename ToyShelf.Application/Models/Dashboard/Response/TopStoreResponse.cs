using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class TopStoreResponse
	{
		public Guid StoreId { get; set; }
		public string StoreName { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string PartnerName { get; set; } = string.Empty; 
		public decimal TotalRevenue { get; set; } 
		public int TotalOrders { get; set; }
	}
}
