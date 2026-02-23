using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.CommissionPolicy.Response
{
	public class CommissionPolicyResponse
	{
		public Guid Id { get; set; }
		public Guid PartnerTierId { get; set; }
		public string PartnerTierName { get; set; } = string.Empty;
		public Guid PriceSegmentId { get; set; }
		public string PriceSegmentName { get; set; } = string.Empty;
		public decimal CommissionRate { get; set; }
		public DateTime? EffectiveDate { get; set; }
	}
}
