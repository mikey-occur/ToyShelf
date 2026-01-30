using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Request
{
	public class GoogleLoginRequest
	{
		public string IdToken { get; set; } = string.Empty;
	}
}
