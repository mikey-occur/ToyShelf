using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;
using PayOS;
using ToyShelf.Application.Auth;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Notifications;
using ToyShelf.Application.QRcode;
using ToyShelf.Application.Security;
using ToyShelf.Application.Services;
using ToyShelf.Application.Translation;
using ToyShelf.Domain.Common.Commission;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Auth;
using ToyShelf.Infrastructure.Common.Payment;
using ToyShelf.Infrastructure.Common.QrCode;
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

			// ===== PayOS Client =====
			services.Configure<ToyShelf.Application.Models.Payment.PayOSOptions>(configuration.GetSection("PayOS"));
			services.AddSingleton<PayOSClient>(sp =>
			{
				var options = sp.GetRequiredService<IOptions<ToyShelf.Application.Models.Payment.PayOSOptions>>().Value;
				return new PayOSClient(options.ClientId, options.ApiKey, options.ChecksumKey);
			});



			// ===== Hangfire =====
			services.AddHangfire(config => config
				.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UsePostgreSqlStorage(options =>
				{
					options.UseNpgsqlConnection(configuration.GetConnectionString("PostgreSql"));
				}));
			services.AddHangfireServer();
			
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
         	services.AddScoped<IShelfRepository, ShelfRepository>();
			services.AddScoped<IPriceSegmentRepository, PriceSegmentRepository>();
			services.AddScoped<IPartnerTierRepository, PartnerTierRepository>();
			services.AddScoped<IPriceTableRepository, PriceTableRepository>();
			services.AddScoped<ICommissionPolicyRepository, CommissionPolicyRepository>();
			services.AddScoped<ICityRepository, CityRepository>();
			services.AddScoped<IPriceTableApplyRepository, PriceTableApplyRepository>();
			services.AddScoped<IStoreCreationRequestRepository, StoreCreationRequestRepository>();
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<IPriceItemRepository, PriceItemRepository>();
			services.AddScoped<ICommissionHistoryRepsitory, CommissionHistoryRepsitory>();
			services.AddScoped<IInventoryLocationRepository, InventoryLocationRepository>();
			services.AddScoped<IInventoryRepository, InventoryRepository>();
			services.AddScoped<IStoreOrderRepository, StoreOrderRepository>();
			services.AddScoped<IMonthlySettlementRepository, MonthlySettlementRepository>();
			services.AddScoped<IShipmentAssignmentRepository, ShipmentAssignmentRepository>();
			services.AddScoped<IShipmentItemRepository, ShipmentItemRepository>();
			services.AddScoped<IShipmentMediaRepository, ShipmentMediaRepository>();
			services.AddScoped<IShipmentRepository, ShipmentRepository>();
			
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
			services.AddScoped<IPriceTableService, PriceTableService>();
			services.AddScoped<ICommissionPolicyService, CommissionPolicyService>();
			services.AddScoped<IShelfService, ShelfService>();
			services.AddScoped<IQrCodeService, QrCodeService>();
			services.AddScoped<IPriceTableApplyService, PriceTableApplyService>();
			services.AddScoped<ICityService, CityService>();
			services.AddScoped<ICommissionService, CommissionService>();
			services.AddScoped<IPaymentService, PayOSPaymentService>();
			services.AddScoped<IOrderService, OrderService>();
			services.AddScoped<IStoreCreationRequestService, StoreCreationRequestService>();
			services.AddScoped<IInventoryService, InventoryService>();
			services.AddScoped<IInventoryLocationService, InventoryLocationService>();
			services.AddScoped<IStoreOrderService, StoreOrderService>();
			services.AddScoped<IMonthlySettlementService, MonthlySettlementService>();
			services.AddScoped<IShipmentAssignmentService, ShipmentAssignmentService>();
			services.AddScoped<IShipmentService, ShipmentService>();
		}
    }
}

