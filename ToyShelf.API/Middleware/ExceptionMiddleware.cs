using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ToyShelf.Application.Common;

namespace ToyShelf.API.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionMiddleware> _logger;
		private readonly IWebHostEnvironment _environment;
		private readonly bool _includeErrorDetail;

		public ExceptionMiddleware(
			RequestDelegate next,
			ILogger<ExceptionMiddleware> logger,
			IWebHostEnvironment environment)
		{
			_next = next;
			_logger = logger;
			_environment = environment;
			_includeErrorDetail = _environment.IsDevelopment() ||
				string.Equals(
					Environment.GetEnvironmentVariable("ENABLE_DETAILED_ERRORS"),
					"true",
					StringComparison.OrdinalIgnoreCase);
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private Task HandleExceptionAsync(HttpContext context, Exception ex)
		{
			_logger.LogError(
				ex,
				"Unhandled exception. Method={Method} Path={Path} TraceId={TraceId}",
				context.Request.Method,
				context.Request.Path,
				context.TraceIdentifier);

			if (context.Response.HasStarted)
			{
				_logger.LogWarning("Response already started, cannot write error response.");
				return Task.CompletedTask;
			}

			int statusCode = (int)HttpStatusCode.InternalServerError;
			string message = "Đã xảy ra lỗi không xác định.";
			string? errorCode = null;

			if (ex is InvalidException invalidEx)
			{
				statusCode = invalidEx.StatusCode;
				message = invalidEx.Message;
				errorCode = invalidEx.ErrorCode;
			}
			else if (ex is AppException appEx)
			{
				statusCode = appEx.StatusCode;
				message = appEx.Message;
				errorCode = appEx.ErrorCode;
			}

			object response;
			if (_includeErrorDetail)
			{
				response = new
				{
					error = message,
					code = errorCode,
					status = statusCode,
					detail = ex.ToString(),
					traceId = context.TraceIdentifier
				};
			}
			else
			{
				response = new
				{
					error = message,
					code = errorCode,
					status = statusCode
				};
			}

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = statusCode;
			return context.Response.WriteAsync(JsonSerializer.Serialize(response));
		}
	}
}
