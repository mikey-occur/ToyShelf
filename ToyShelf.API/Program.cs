using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using ToyShelf.API.Configuration;
using ToyShelf.API.Middleware;
using ToyShelf.Infrastructure.Common.Time;
using ToyShelf.Infrastructure.Context;


var builder = WebApplication.CreateBuilder(args);

// ===== Modular DI =====
builder.Services.AddDbContexts(builder.Configuration.GetConnectionString("PostgreSql"));
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
		var context = scope.ServiceProvider.GetRequiredService<ToyCabinDbContext>();
		context.Database.Migrate();
		Console.WriteLine("Database migration applied successfully.");
	}
}
catch (Exception ex)
{
	Console.WriteLine($"An error occurred during database migration: {ex.Message}");
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AllowAll");

//app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();