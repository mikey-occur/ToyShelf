using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Infrastructure.Auth
{
	public class JwtOptions
	{
		public string Issuer { get; set; } = string.Empty;
		public string Audience { get; set; } = string.Empty;
		public string SecretKey { get; set; } = string.Empty;
		public int AccessTokenMinutes { get; set; }
	}
}
