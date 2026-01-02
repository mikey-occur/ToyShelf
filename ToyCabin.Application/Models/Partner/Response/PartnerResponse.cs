using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Partner.Response
{
	public class PartnerResponse
	{
		public Guid Id { get; set; }
		public string CompanyName { get; set; } = string.Empty;
		public string Tier { get; set; } = string.Empty;
		public decimal RevenueSharePercent { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
