namespace ToyShelf.API.Configuration
{
	public static class CorsExtensions
	{
		public static void AddCorsPolicies(this IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAll", policy =>
				{
					policy.AllowAnyHeader()
						  .AllowAnyMethod()
						  .AllowCredentials()
						  .SetIsOriginAllowed(_ => true);
				});
			});
		}
	}
}
