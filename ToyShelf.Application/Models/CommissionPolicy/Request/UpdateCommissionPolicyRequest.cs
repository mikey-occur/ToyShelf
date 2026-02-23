using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.CommissionPolicy.Request
{
	public class UpdateCommissionPolicyRequest
	{
		public decimal CommissionRate { get; set; }
		public DateTime? EffectiveDate { get; set; }
	}
}
