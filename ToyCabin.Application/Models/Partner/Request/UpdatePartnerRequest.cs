using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Partner.Request
{
	public class UpdatePartnerRequest
	{
		public string CompanyName { get; set; } = string.Empty;
		public string Tier { get; set; } = "STANDARD";
		public decimal RevenueSharePercent { get; set; }
	}
}
