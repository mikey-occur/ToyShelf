using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Partner.Request
{
	public class CreatePartnerRequest
	{
		public Guid PartnerTierId { get; set; }
		public string CompanyName { get; set; } = string.Empty;
	}
}
