using Microsoft.EntityFrameworkCore;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.API.Configuration
{
	public static class DbContextExtensions
	{
		public static void AddDbContexts(this IServiceCollection services, string? connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException(nameof(connectionString), "Connection string is null or empty.");

			services.AddDbContext<ToyShelfDbContext>(options =>
				options.UseNpgsql(connectionString));
		}

	}
}
