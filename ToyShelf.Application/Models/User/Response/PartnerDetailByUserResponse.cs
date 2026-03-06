using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.User.Response
{
	public class PartnerDetailByUserResponse
	{
		public Guid UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;

		public Guid? PartnerId { get; set; }
		public string? CompanyName { get; set; }
		public bool? PartnerIsActive { get; set; }
	}
}
