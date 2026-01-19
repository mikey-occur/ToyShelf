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
			services.AddScoped<ICurrentUser, CurrentUser>();

			// ===== Common =====
			services.AddSingleton<IDateTimeProvider, VietnamDateTimeProvider>();

			// ===== Repositories =====
			services.AddScoped<IRoleRepository, RoleRepository>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IPasswordResetOtpRepository, PasswordResetOtpRepository>();
			services.AddScoped<ICabinRepository, CabinRepository>();
			services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IPartnerRepository, PartnerRepository>();
			services.AddScoped<IStoreRepository, StoreRepository>();
			services.AddScoped<IUserStoreRepository, UserStoreRepository>();
			services.AddScoped<IStoreInvitationRepository, StoreInvitationRepository>();
			services.AddScoped<IWarehouseRepository, WarehouseRepository>();

			// ===== Services =====
			services.AddScoped<IRoleService, RoleService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
			services.AddScoped<ICabinService, CabinService>();
			services.AddScoped<IProductCategoryService, ProductCategoryService>();
			services.AddScoped<IProductService, ProductService>();
			services.AddScoped<IPartnerService, PartnerService>();
			services.AddScoped<IStoreService, StoreService>();
			services.AddScoped<IUserStoreService, UserStoreService>();
			services.AddScoped<IStoreInvitationService, StoreInvitationService>();
			services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IProductBroadcaster, SignalRService>();
        }
	}
}
