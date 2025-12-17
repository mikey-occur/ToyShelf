using ToyCabin.Application.Auth;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Services;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.IRepositories;
using ToyCabin.Infrastructure.Auth;
using ToyCabin.Infrastructure.Common.Time;
using ToyCabin.Infrastructure.Repositories;

namespace ToyCabin.API.Configuration
{
	public static class ServiceRegistrationExtensions
	{
		public static void AddAppServices(this IServiceCollection services)
		{
			// ===== Unit of Work & Generic =====
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			// ===== Common =====
			services.AddSingleton<IDateTimeProvider, VietnamDateTimeProvider>();

			// ===== Repositories =====

			// ===== Services =====
			services.AddScoped<IRoleService, RoleService>();
			services.AddScoped<ITokenService, TokenService>();

		}
	}
}
