using Microsoft.EntityFrameworkCore;
using ToyCabin.Infrastructure.Context;

namespace ToyCabin.API.Configuration
{
	public static class DbContextExtensions
	{
		public static void AddDbContexts(this IServiceCollection services, string? connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException(nameof(connectionString), "Connection string is null or empty.");

			services.AddDbContext<ToyCabinDbContext>(options =>
				options.UseNpgsql(connectionString));
		}

	}
}
