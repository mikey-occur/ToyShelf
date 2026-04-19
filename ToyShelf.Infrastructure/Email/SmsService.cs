using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Notifications;
using ToyShelf.Domain.IRepositories;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ToyShelf.Infrastructure.Email
{
	public class SmsService : ISmsService
	{
		private readonly IConfiguration _configuration;
		private readonly IOrderRepository _orderRepository;
		private readonly IHttpClientFactory _httpClientFactory;
		public SmsService(IConfiguration configuration, IOrderRepository orderRepository, IHttpClientFactory httpClientFactory)
		{
			_configuration = configuration;
			_orderRepository = orderRepository;
			_httpClientFactory = httpClientFactory;
		}

		//public async Task SendPaymentSuccessSmsAsync(string phoneNumber, long orderCode)
		//{
		//	// 1. Kéo data hóa đơn từ DB lên (Kéo luôn OrderItems và ProductColor)
		//	var order = await _orderRepository.GetOrderWithItemsAndStoreAsync(orderCode);
		//	if (order == null) return;

		//	// 2. Lấy cấu hình Twilio
		//	var accountSid = _configuration["TwilioSms:AccountSid"];
		//	var authToken = _configuration["TwilioSms:AuthToken"];
		//	var fromNumber = _configuration["TwilioSms:FromPhoneNumber"];

		//	TwilioClient.Init(accountSid, authToken);

		//	// 3. Chuẩn bị nội dung hóa đơn bằng StringBuilder để xuống dòng
		//	var sb = new StringBuilder();
		//	sb.AppendLine($"ToyShelf: Thanh toan thanh cong don {orderCode}");
		//	sb.AppendLine("Chi tiet:");

		//	// Lặp qua từng món hàng
		//	foreach (var item in order.OrderItems)
		//	{

		//		var itemTotal = item.Price * item.Quantity;
		//		sb.AppendLine($"- {item.Quantity}x {item.ProductColor?.Sku}: {itemTotal:N0}d");
		//	}

		//	sb.AppendLine($"Tong tien: {order.TotalAmount:N0}d");
		//	sb.Append("Cam on quy khach!");

		//	var messageContent = sb.ToString();

		//	phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "").Trim();

		//	if (phoneNumber.StartsWith("0"))
		//	{
		//		phoneNumber = "+84" + phoneNumber.Substring(1);
		//	}
		//	else if (!phoneNumber.StartsWith("+"))
		//	{
		//		phoneNumber = "+" + phoneNumber;
		//	}

		//	// 5. Gửi SMS
		//	await MessageResource.CreateAsync(
		//		body: messageContent,
		//		from: new PhoneNumber(fromNumber),
		//		to: new PhoneNumber(phoneNumber)
		//	);
		//}

		public async Task SendPaymentSuccessSmsAsync(string phoneNumber, long orderCode)
		{

			var order = await _orderRepository.GetOrderWithItemsAndStoreAsync(orderCode);
			if (order == null) return;

			var apiKey = _configuration["Infobip:ApiKey"];
			var baseUrl = _configuration["Infobip:BaseUrl"];
			var sender = _configuration["Infobip:Sender"];

			var sb = new StringBuilder();
			sb.AppendLine($"ToyShelf: Thanh toan thanh cong #{orderCode}");

			foreach (var item in order.OrderItems)
			{
				var itemTotal = item.Price * item.Quantity;
				sb.AppendLine($"- {item.Quantity}x {item.ProductColor?.Sku}: {itemTotal:N0}d");
			}
			sb.AppendLine($"Tong: {order.TotalAmount:N0}d");
			sb.Append("Cam on ban!");

			var messageContent = sb.ToString();

			var finalPhone = phoneNumber.Replace(" ", "").Replace("-", "").Trim();

			if (finalPhone.StartsWith("0"))
			{
				finalPhone = "84" + finalPhone.Substring(1);
			}
			else if (finalPhone.StartsWith("+84"))
			{
				finalPhone = finalPhone.Substring(1);
			}
			else if (!finalPhone.StartsWith("84"))
			{
				finalPhone = "84" + finalPhone;
			}

			// 5. Gửi qua Infobip bằng HttpClient
			var client = _httpClientFactory.CreateClient();
			client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"App {apiKey}");

			var payload = new
			{
				messages = new[]
				{
			new {
				from = sender,
				destinations = new[] { new { to = finalPhone } },
				text = messageContent
			}
		}
			};

			try
			{
				var response = await client.PostAsJsonAsync($"{baseUrl}/sms/2/text/advanced", payload);

				if (response.IsSuccessStatusCode)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"[Infobip] SMS sent successfully for Order #{orderCode}");
					Console.ResetColor();
				}
				else
				{
					var errorDetail = await response.Content.ReadAsStringAsync();
					throw new Exception($"Infobip Error: {errorDetail}");
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"[SMS ERROR] {ex.Message}");
				Console.ResetColor();
				throw;
			}
		}
	}
}

