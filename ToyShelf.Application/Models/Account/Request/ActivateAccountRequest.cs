using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Request
{
	public class ActivateAccountRequest
	{
		public string Email { get; set; } = string.Empty;
		public string OtpCode { get; set; } = null!;
		public string NewPassword { get; set; } = null!;
		public string ConfirmPassword { get; set; } = null!;
	}
}
