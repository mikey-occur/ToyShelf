using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.CommissionPolicy.Request
{
	public class CommissionPolicyRequest
	{
		public Guid PartnerTierId { get; set; }
		public Guid PriceSegmentId { get; set; }
		public decimal CommissionRate { get; set; }
		public DateTime? EffectiveDate { get; set; }
	}
}
