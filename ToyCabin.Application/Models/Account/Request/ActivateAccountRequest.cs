using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Account.Request
{
	public class ActivateAccountRequest
	{
		public string OtpCode { get; set; } = null!;
		public string NewPassword { get; set; } = null!;
	}
}
