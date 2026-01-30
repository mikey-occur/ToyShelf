using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Response
{
	public class ActivateAccountResponse
	{
		public string Email { get; set; } = string.Empty;
		public DateTime LastLoginAt { get; set; }
	}

}
