using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.PartnerTier.Request
{
	public class PartnerTierRequest
	{
		public string Name { get; set; } = null!;
		public int Priority { get; set; }
	}
}
