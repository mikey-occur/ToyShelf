using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Response
{
	public class LoginResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public List<string> Roles { get; set; } = new();
		public DateTime LastLoginAt { get; set; }
	}
}
