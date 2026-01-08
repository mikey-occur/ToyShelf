using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ToyCabin.Application.Auth;
using System.IdentityModel.Tokens.Jwt;


namespace ToyCabin.Infrastructure.Auth
{
	public class CurrentUser : ICurrentUser
	{
		public Guid AccountId { get; }
		public Guid UserId { get; }
		public Guid? PartnerId { get; }
		public IReadOnlyList<string> Roles { get; }

		public CurrentUser(IHttpContextAccessor accessor)
		{
			var user = accessor.HttpContext?.User
				?? throw new UnauthorizedAccessException("Unauthenticated");

			AccountId = Guid.Parse(
				user.FindFirst("aid")?.Value
				?? throw new UnauthorizedAccessException("Missing aid claim"));

			UserId = Guid.Parse(
				user.FindFirst(ClaimTypes.NameIdentifier)?.Value
				?? throw new UnauthorizedAccessException("Missing user id claim"));

			var partnerId = user.FindFirst("partnerid")?.Value;
			PartnerId = string.IsNullOrWhiteSpace(partnerId)
				? null
				: Guid.Parse(partnerId);

			Roles = user.FindAll(ClaimTypes.Role)
						.Select(x => x.Value)
						.ToList();
		}

		public bool IsPartnerAdmin()
			=> Roles.Contains("PartnerAdmin");
			
		public bool IsPartner()
			=> Roles.Contains("Partner");

		public bool IsAdmin()
			=> Roles.Contains("Admin");
	}
}
