namespace ToyCabin.API.Configuration
{
	public static class SwaggerExtensions
	{
		public static void AddSwaggerSetup(this IServiceCollection services)
		{
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}
	}
}
