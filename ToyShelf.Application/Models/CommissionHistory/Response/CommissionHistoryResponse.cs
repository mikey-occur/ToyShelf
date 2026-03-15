using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.CommissionHistory.Response
{
	public class CommissionHistoryResponse
	{
		public Guid Id { get; set; }
		public Guid OrderItemId { get; set; }

		public decimal AppliedRate { get; set; }

		public decimal CommissionAmount { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
