using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;


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
		public static Guid GetUserId(this ClaimsPrincipal user)
		{
			var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
				throw new UnauthorizedAccessException("UserId not found in token");

			return Guid.Parse(userId);
		}
		public static string GetProvider(this ClaimsPrincipal user)
		{
			var provider = user.FindFirst("prov")?.Value;

			if (string.IsNullOrEmpty(provider))
				throw new UnauthorizedAccessException("Provider not found in token");

			return provider;
		}
	}
}
