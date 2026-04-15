using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Options;
using PayOS;
using StackExchange.Redis;
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
using ToyShelf.Infrastructure.Common.ExportExcel;
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
					options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
				}));
			services.AddHangfireServer();

			// Lấy ConnectionString từ appsettings.json
			var redisConnection = configuration.GetConnectionString("Redis");
			if (!string.IsNullOrEmpty(redisConnection))
			{

				services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));
			}

			services.AddStackExchangeRedisCache(options =>
				{
					options.Configuration = redisConnection;
					options.InstanceName = "ToyShelf_";
				});

			services.AddSignalR()
			.AddStackExchangeRedis(redisConnection!, options =>
			{

				options.Configuration.ChannelPrefix = RedisChannel.Literal("ToyShelf_SignalR");
			});

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
			services.AddScoped<IPartnerTierRepository, PartnerTierRepository>();
			services.AddScoped<ICommissionTableRepository, CommissionTableRepository>();
			services.AddScoped<ICityRepository, CityRepository>();
			services.AddScoped<ICommissionTableApplyRepository, CommissionTableApplyRepository>();
			services.AddScoped<IStoreCreationRequestRepository, StoreCreationRequestRepository>();
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<ICommissionItemRepository, CommissiontemRepository>();
			services.AddScoped<ICommissionHistoryRepsitory, CommissionHistoryRepsitory>();
			services.AddScoped<IInventoryLocationRepository, InventoryLocationRepository>();
			services.AddScoped<IInventoryRepository, InventoryRepository>();
			services.AddScoped<IStoreOrderRepository, StoreOrderRepository>();
			services.AddScoped<IMonthlySettlementRepository, MonthlySettlementRepository>();
			services.AddScoped<IShipmentAssignmentRepository, ShipmentAssignmentRepository>();
			services.AddScoped<IShipmentItemRepository, ShipmentItemRepository>();
			services.AddScoped<IShipmentMediaRepository, ShipmentMediaRepository>();
			services.AddScoped<IShipmentRepository, ShipmentRepository>();
			services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
			services.AddScoped<IUserWarehouseRepository, UserWarehouseRepository>();
			services.AddScoped<IShelfTypeRepository, ShelfTypeRepository>();
			services.AddScoped<IShelfRepository, ShelfRepository>();
			services.AddScoped<IShelfOrderRepository, ShelfOrderRepository>();
			services.AddScoped<IShelfOrderItemRepository, ShelfOrderItemRepository>();
			services.AddScoped<IShelfShipmentItemRepository, ShelfShipmentItemRepository>();
			services.AddScoped<IShelfTransactionRepository, ShelfTransactionRepository>();
			services.AddScoped<INotificationRepository, NotificationRepository>();
			services.AddScoped<INotificationBroadcaster, SignalRService>();
			services.AddScoped<IDamageReportRepository, DamageReportRepository>();
			services.AddScoped<IInventoryShelfRepository, InventoryShelfRepository>();

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
			services.AddScoped<IPartnerTierService, PartnerTierService>();
			services.AddScoped<ICommissionTableService, CommissionTableService>();
			services.AddScoped<IShelfService, ShelfService>();
			services.AddScoped<IQrCodeService, QrCodeService>();
			services.AddScoped<ICommissionTableApplyService, CommissionTableApplyService>();
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
			services.AddScoped<IExcelService, ExcelService>();
			services.AddScoped<IShelfTypeService, ShelfTypeService>();
			services.AddScoped<IDashboardService, DashboardService>();
			services.AddScoped<IShelfOrderService, ShelfOrderService>();
			services.AddScoped<INotificationService, NotificationService>();
			services.AddScoped<IRedisCacheService, RedisCacheService>();
			services.AddScoped<IDamageReportService, DamageReportService>();
			services.AddScoped<ICommissionHistoryService, CommissionHistoryService>();
			services.AddScoped<IInventoryShelfService, InventoryShelfService>();
		}
    }
}

