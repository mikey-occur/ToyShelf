using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Account.Request
{
	public class SetLocalPasswordRequest
	{
		public string NewPassword { get; set; } = null!;
		public string ConfirmPassword { get; set; } = null!;
	}
}
