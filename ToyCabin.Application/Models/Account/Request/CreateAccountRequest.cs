using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Account.Request
{
	public class CreateAccountRequest
	{
		public string Email { get; set; } = null!;
		public string FullName { get; set; } = null!;
		public List<Guid> RoleIds { get; set; } = new();
	}
}
