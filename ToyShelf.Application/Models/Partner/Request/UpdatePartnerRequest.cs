using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Partner.Request
{
	public class UpdatePartnerRequest
	{
		public Guid PartnerTierId { get; set; }
		public string CompanyName { get; set; } = string.Empty;
		public string? BankName { get; set; }
		public string? BankAccountNumber { get; set; }
		public string? BankAccountName { get; set; }
        public string Address { get; set; } = string.Empty;
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
	}
}
