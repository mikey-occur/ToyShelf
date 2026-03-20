using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.MonthlySettlement.Request
{
	public class UpdateDeductionRequest
	{
		public decimal DeductionAmount { get; set; } = 0;
		public string? Note { get; set; }
	}
}
