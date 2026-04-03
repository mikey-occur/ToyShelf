using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Request
{
	public class PartnerChartRequest
	{
		public string ViewType { get; set; } = "month"; // week, month, year
		public int? Month { get; set; }
		public int? Year { get; set; }
	}
}
