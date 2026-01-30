using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Request
{
	public class CreatePartnerUserRequest
	{
		public string Email { get; set; } = null!;
		public string FullName { get; set; } = null!;
	}
}
