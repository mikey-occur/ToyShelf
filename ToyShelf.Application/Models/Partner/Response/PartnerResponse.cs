using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Partner.Response
{
	public class PartnerResponse
	{
		public Guid Id { get; set; }
		public string CompanyName { get; set; } = string.Empty;

		public Guid PartnerTierId { get; set; }
		public string PartnerTierName { get; set; } = string.Empty;
		public int PartnerTierPriority { get; set; }

		public bool IsActive { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
