using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToyShelf.Infrastructure.Auth;

namespace ToyShelf.API.Configuration
{
	public static class JwtServiceExtensions
	{
		public static IServiceCollection AddJwtAuthentication(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			services.Configure<JwtOptions>(
				configuration.GetSection("Jwt"));

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					var jwt = configuration
						.GetSection("Jwt")
						.Get<JwtOptions>();

					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,

						ValidIssuer = jwt!.Issuer,
						ValidAudience = jwt.Audience,
						IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(jwt.SecretKey)),

						ClockSkew = TimeSpan.Zero
					};

					// =============================
					// CUSTOM RESPONSE 401 / 403
					// =============================
					options.Events = new JwtBearerEvents
					{
						OnChallenge = context =>
						{
							context.HandleResponse();

							context.Response.StatusCode = StatusCodes.Status401Unauthorized;
							context.Response.ContentType = "application/json";

							var response = new
							{
								success = false,
								code = "Unauthorized",
								message = "You must login to access this resource"
							};

							return context.Response.WriteAsJsonAsync(response);
						},

						OnForbidden = context =>
						{
							context.Response.StatusCode = StatusCodes.Status403Forbidden;
							context.Response.ContentType = "application/json";

							var response = new
							{
								success = false,
								code = "Forbidden",
								message = "You do not have permission to access this resource"
							};

							return context.Response.WriteAsJsonAsync(response);
						}
					};
				});

			return services;
		}
	}
}
