using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.User.Request
{
	public class UpdateUserRequest
	{
		public string FullName { get; set; } = string.Empty;
		public string? AvatarUrl { get; set; }
	}

}
