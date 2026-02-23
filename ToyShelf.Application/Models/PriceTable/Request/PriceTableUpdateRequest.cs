using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.PriceTable.Request
{
	public class PriceTableUpdateRequest
	{
		public string Name { get; set; } = null!;
		public Guid? PartnerTierId { get; set; }
		public PriceTableType Type { get; set; }
		public bool IsActive { get; set; }

		public List<PriceItemRequest> Items { get; set; } = new();
	}
}
