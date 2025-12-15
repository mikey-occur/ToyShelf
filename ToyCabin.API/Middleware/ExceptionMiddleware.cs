using System.Net;
using System.Text.Json;
using ToyCabin.Application.Common;

namespace ToyCabin.API.Middleware
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;

		public ExceptionMiddleware(RequestDelegate next)
		{
			_next = next;
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

		private static Task HandleExceptionAsync(HttpContext context, Exception ex)
		{
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

			var response = new
			{
				error = message,
				code = errorCode,
				status = statusCode
			};

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = statusCode;
			return context.Response.WriteAsync(JsonSerializer.Serialize(response));
		}
	}
}
