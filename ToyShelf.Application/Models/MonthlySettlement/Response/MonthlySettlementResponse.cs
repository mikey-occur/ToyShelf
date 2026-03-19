using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.CommissionHistory.Response;

namespace ToyShelf.Application.Models.MonthlySettlement.Response
{
	public class MonthlySettlementResponse
	{
		public Guid Id { get; set; }
		public Guid PartnerId { get; set; }
		public string? PartnerName { get; set; } 	
		public int Month { get; set; }
		public int Year { get; set; }
		public int TotalItems { get; set; }
		public decimal TotalCommissionAmount { get; set; }
		public decimal DeductionAmount { get; set; } = 0;
		public decimal FinalAmount { get; set; }
		public string? Note { get; set; }
		public string Status { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }

		public List<CommissionHistoryResponse> Histories { get; set; } = new List<CommissionHistoryResponse>();
	}
}
