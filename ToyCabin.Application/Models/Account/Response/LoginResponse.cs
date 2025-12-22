using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Account.Response
{
	public class LoginResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public DateTime LastLoginAt { get; set; }
	}
}
