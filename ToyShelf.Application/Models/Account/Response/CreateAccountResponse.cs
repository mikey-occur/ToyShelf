using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Response
{
	public class CreateAccountResponse
	{
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
	}

}
