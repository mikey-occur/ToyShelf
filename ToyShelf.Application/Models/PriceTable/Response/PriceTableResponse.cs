using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.PriceTable.Response
{
	public class PriceTableResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; } 
		public Guid? PartnerTierId { get; set; }
		public string? PartnerTierName { get; set; } 
		public bool IsActive { get; set; }

		public List<PriceItemResponse> Items { get; set; } = new();

		public class PriceItemResponse
		{
			public Guid Id { get; set; }
			public Guid PriceSegmentId { get; set; }
			public string PriceSegmentName { get; set; } 
			public decimal CommissionRate { get; set; }
		}
	}
}
