using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.PriceTable.Request
{
	public class CommissionTableUpdateRequest
	{
		public string Name { get; set; } = null!;
		public Guid? PartnerTierId { get; set; }
		public CommissionTableType Type { get; set; }
		public bool IsActive { get; set; }

		public List<CommissionItemRequest> Items { get; set; } = new();
	}
}
