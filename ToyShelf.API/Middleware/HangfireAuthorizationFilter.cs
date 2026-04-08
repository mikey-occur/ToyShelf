using Hangfire.Dashboard;

namespace ToyShelf.API.Middleware
{
	public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
			
			var httpContext = context.GetHttpContext();


			return true;

			//return httpContext.User.Identity?.IsAuthenticated == true && 
   //                httpContext.User.IsInRole("Admin");
            
		}
	}
}
