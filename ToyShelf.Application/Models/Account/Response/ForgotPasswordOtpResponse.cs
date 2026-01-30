using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Response
{
	public class ForgotPasswordOtpResponse
	{
		public string Email { get; set; } = string.Empty;
		public DateTime ExpiredAt { get; set; }
	}
}
