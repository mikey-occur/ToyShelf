using ToyShelf.Application.Auth;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Notifications;
using ToyShelf.Application.Security;
using ToyShelf.Application.Services;
using ToyShelf.Application.Translation;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Auth;
using ToyShelf.Infrastructure.Common.Time;
using ToyShelf.Infrastructure.Common.Translation;
using ToyShelf.Infrastructure.Email;
using ToyShelf.Infrastructure.Repositories;
using ToyShelf.Infrastructure.Security;

namespace ToyShelf.API.Configuration
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
			services.AddScoped<ITranslationService, TranslationService>();

			// ===== Repositories =====
			services.AddScoped<IRoleRepository, RoleRepository>();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IPasswordResetOtpRepository, PasswordResetOtpRepository>();
			services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IPartnerRepository, PartnerRepository>();
			services.AddScoped<IStoreRepository, StoreRepository>();
			services.AddScoped<IUserStoreRepository, UserStoreRepository>();
			services.AddScoped<IStoreInvitationRepository, StoreInvitationRepository>();
			services.AddScoped<IWarehouseRepository, WarehouseRepository>();
			services.AddScoped<IProductColorRepository, ProductColorRepository>();
			services.AddScoped<IColorRepository, ColorRepository>();
			services.AddScoped<IPriceSegmentRepository, PriceSegmentRepository>();
			services.AddScoped<IPartnerTierRepository, PartnerTierRepository>();
			// ===== Services =====
			services.AddScoped<IRoleService, RoleService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
			services.AddScoped<IProductCategoryService, ProductCategoryService>();
			services.AddScoped<IProductService, ProductService>();
			services.AddScoped<IPartnerService, PartnerService>();
			services.AddScoped<IStoreService, StoreService>();
			services.AddScoped<IUserStoreService, UserStoreService>();
			services.AddScoped<IStoreInvitationService, StoreInvitationService>();
			services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IProductBroadcaster, SignalRService>();
			services.AddScoped<IProductColorService, ProductColorService>();
			services.AddScoped<IColorService, ColorService>();
			services.AddScoped<IPriceSegmentService, PriceSegmentService>();
			services.AddScoped<IPartnerTierService, PartnerTierService>();
		}
			
	}
}
