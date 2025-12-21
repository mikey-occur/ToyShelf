using Microsoft.EntityFrameworkCore;
using ToyCabin.API.Configuration;
using ToyCabin.API.Middleware;
using ToyCabin.Infrastructure.Common.Time;
using ToyCabin.Infrastructure.Context;

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
		opt.JsonSerializerOptions.Converters
			.Add(new VietnamDateTimeJsonConverter());
	});


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