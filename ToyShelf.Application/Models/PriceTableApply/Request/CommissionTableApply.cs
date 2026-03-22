using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.PriceTableApply.Request
{
	public class CommissionTableApply
	{
		public Guid PartnerId { get; set; }
		public Guid PriceTableId { get; set; }
		public string? Name { get; set; } 
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; } 
	}
}
