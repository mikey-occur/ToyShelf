
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class TopPartnerResponse
	{
		public Guid PartnerId { get; set; }
		public string CompanyName { get; set; } = string.Empty;
		public string ContactName { get; set; } = string.Empty; 
		public string Email { get; set; } = string.Empty;
		public string Tier { get; set; } = string.Empty;        
		public decimal TotalRevenue { get; set; }              
		public decimal TotalCommission { get; set; }
	}
}
