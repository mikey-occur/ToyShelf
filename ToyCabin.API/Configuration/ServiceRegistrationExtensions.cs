using ToyCabin.Application.Auth;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Notifications;
using ToyCabin.Application.Security;
using ToyCabin.Application.Services;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.IRepositories;
using ToyCabin.Infrastructure.Auth;
using ToyCabin.Infrastructure.Common.Time;
using ToyCabin.Infrastructure.Email;
using ToyCabin.Infrastructure.Repositories;
using ToyCabin.Infrastructure.Security;

namespace ToyCabin.API.Configuration
{
	public static class ServiceRegistrationExtensions
	{
		public static void AddAppServices(this IServiceCollection services, IConfiguration configuration)
		{
			// ===== Email =====
			services.Configure<EmailOptions>(
				configuration.GetSection("Email"));
			services.AddScoped<IEmailService, SmtpEmailService>();

			// ===== Unit of Work & Generic =====
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			// ===== Security =====
			services.AddScoped<IPasswordHasher, PasswordHasher>();
			services.AddScoped<ITokenService, TokenService>();

			// ===== Common =====
			services.AddSingleton<IDateTimeProvider, VietnamDateTimeProvider>();

			// ===== Repositories =====
			services.AddScoped<IRoleRepository, RoleRepository>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IPasswordResetOtpRepository, PasswordResetOtpRepository>();

			// ===== Services =====
			services.AddScoped<IRoleService, RoleService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<IUserService, UserService>();

		}
	}
}
