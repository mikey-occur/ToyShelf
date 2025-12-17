using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Auth;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Infrastructure.Auth
{
	public class TokenService : ITokenService
	{
		private readonly JwtOptions _jwt;

		public TokenService(IOptions<JwtOptions> options)
		{
			_jwt = options.Value;
		}

		public string GenerateAccessToken(Account account)
		{
			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
				new(JwtRegisteredClaimNames.Email, account.Email),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new(JwtRegisteredClaimNames.Iat,
					DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
					ClaimValueTypes.Integer64),

				new Claim(ClaimTypes.Name, account.Username)
			};

			foreach (var accountRole in account.AccountRoles)
			{
				if (accountRole.Role != null && accountRole.Role.IsActive)
				{
					claims.Add(
						new Claim(ClaimTypes.Role, accountRole.Role.Name)
					);
				}
			}

			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_jwt.SecretKey));

			var creds = new SigningCredentials(
				key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwt.Issuer,
				audience: _jwt.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}

}
