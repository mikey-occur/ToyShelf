namespace ToyCabin.API.Configuration
{
	public static class ServiceRegistrationExtensions
	{
		public static void AddAppServices(this IServiceCollection services)
		{
			// ===== Repositories =====
			// Nếu bạn có nhiều repo, register ở đây
			// services.AddScoped<IAccountRepository, AccountRepository>();
			// services.AddScoped<IRoleRepository, RoleRepository>();

			// ===== Services =====
			// services.AddScoped<IAccountService, AccountService>();
			// services.AddScoped<IRoleService, RoleService>();
		}
	}
}
