using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Common.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		public static Guid GetAccountId(this ClaimsPrincipal user)
		{
			var accountId = user.FindFirst("aid")?.Value;

			if (string.IsNullOrEmpty(accountId))
				throw new UnauthorizedAccessException("AccountId not found in token");

			return Guid.Parse(accountId);
		}
	}
}
