using Hangfire;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json.Serialization;
using ToyShelf.API.Configuration;
using ToyShelf.API.Hubs;
using ToyShelf.API.Middleware;
using ToyShelf.Application.IServices;
using ToyShelf.Infrastructure.Common.Time;
using ToyShelf.Infrastructure.Context;

var builder = WebApplication.CreateBuilder(args);

// ===== Modular DI =====
builder.Services.AddDbContexts(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddCorsPolicies();
builder.Services.AddSwaggerSetup();
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services
	.AddControllers()
	.AddJsonOptions(opt =>
	{
		opt.JsonSerializerOptions.Converters.Add(
			new JsonStringEnumConverter()
		);
		opt.JsonSerializerOptions.Converters
			.Add(new VietnamDateTimeJsonConverter()
		);
		opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		opt.JsonSerializerOptions.WriteIndented = false;
	});
builder.Services.AddSwaggerGen(options =>
{
	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	options.IncludeXmlComments(xmlPath);
});

// ===== HttpContext =====
builder.Services.AddHttpContextAccessor();

//===== SignalR =====
builder.Services.AddSignalR();


var app = builder.Build();

// Migration tự động
try
{
	using (var scope = app.Services.CreateScope())
	{
		var context = scope.ServiceProvider.GetRequiredService<ToyShelfDbContext>();
		context.Database.Migrate();
		Console.WriteLine("Database migration applied successfully.");
	}
}
catch (Exception ex)
{
	Console.WriteLine($"An error occurred during database migration: {ex.Message}");
}
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".manifest"] = "text/plain";
provider.Mappings[""] = "application/octet-stream";

// if (app.Environment.IsDevelopment())
// {
// 	app.UseSwagger();
// 	app.UseSwaggerUI();
// }
// Chỉ enable Swagger nếu có env var cho phép
var enableSwagger = app.Environment.IsDevelopment() ||
	Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true";

if (enableSwagger)
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseStaticFiles(new StaticFileOptions
{
	ContentTypeProvider = provider,
	ServeUnknownFileTypes = true,
	DefaultContentType = "application/octet-stream"
});



app.UseMiddleware<ExceptionMiddleware>();

// Enable serving static files from wwwroot (e.g., /robot, /AssetBundles)
// ServeUnknownFileTypes required for files without extensions
app.UseStaticFiles(new StaticFileOptions
{
	ServeUnknownFileTypes = true,
	DefaultContentType = "application/octet-stream"
});

// ===== Hangfire Dashboard =====
// 1. Job chốt toán cuối tháng (Chạy vào 0h ngày 1 hàng tháng)
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<IMonthlySettlementService>(
	"auto-monthly-settlement",
	service => service.GenerateLastMonthSettlementAutoAsync(),
	"0 0 * * *",
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
	}
);
// job xoá notification cũ hơn 30 ngày (Chạy vào 2h sáng hàng ngày)
recurringJobManager.AddOrUpdate<INotificationService>(
	"Cleanup-Old-Notifications",
	service => service.CleanupOldNotificationsAsync(30),
	Cron.Daily(2, 0),
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
	}
);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
	Authorization = new[] { new HangfireAuthorizationFilter() }

});
app.MapHub<ProductHub>("/productHub");
app.MapHub<NotificationHub>("/hubs/notification");

app.MapControllers();

app.Run();