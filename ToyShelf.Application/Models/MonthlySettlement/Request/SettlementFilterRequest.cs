using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.MonthlySettlement.Request
{
	public class SettlementFilterRequest
	{
		public int? Year { get; set; }
		public int? Month { get; set; }
		public Guid? PartnerId { get; set; }
		public string? Status { get; set; }
	}
}
