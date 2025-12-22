using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Account.Response
{
	public class ActivationOtpResponse
	{
		public string Email { get; set; } = string.Empty;
		public DateTime ExpiredAt { get; set; }
	}

}
