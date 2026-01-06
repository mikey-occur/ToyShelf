using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Auth;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Infrastructure.Auth
{
	public class TokenService : ITokenService
	{
		private readonly JwtOptions _jwt;
		private readonly IRoleRepository _roleRepository;

		public TokenService(IOptions<JwtOptions> options, IRoleRepository roleRepository)
		{
			_jwt = options.Value;
			_roleRepository = roleRepository;
		}

		public async Task<string> GenerateAccessTokenAsync(Account account)
		{
			var user = account.User
				?? throw new InvalidOperationException("Account.User must be loaded");

			var claims = new List<Claim>
			{
				// Identity
				new(JwtRegisteredClaimNames.Sub, account.UserId.ToString()),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new(JwtRegisteredClaimNames.Iat,
					DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
					ClaimValueTypes.Integer64),
				new(JwtRegisteredClaimNames.Email, user.Email),

				// Account Info
				new(ClaimTypes.Name, user.FullName),
				new("aid", account.Id.ToString()),
				new("partnerid", user.PartnerId?.ToString() ?? string.Empty),
				new("prov", account.Provider.ToString())
			};

			var roles = await _roleRepository.GetRolesByUserIdAsync(account.UserId);

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role.Name));
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
