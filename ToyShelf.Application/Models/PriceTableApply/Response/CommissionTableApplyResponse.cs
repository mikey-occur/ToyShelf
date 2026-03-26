using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.PriceTableApply.Response
{
	public class CommissionTableApplyResponse
	{
		public Guid Id { get; set; }
		public Guid PartnerId { get; set; }
		public string PartnerName { get; set; } 
		public Guid CommissionTableId { get; set; }
		public string CommissionTableName { get; set; }

		public string? Name { get; set; } 
		public bool IsActive { get; set; } = true;
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; } 
	}
}
