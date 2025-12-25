using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Account.Response
{
	public class SetLocalPasswordResponse
	{
		public string Email { get; set; } = null!;
		public DateTime SetAt { get; set; }
	}
}
