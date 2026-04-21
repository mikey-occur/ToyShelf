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

		//	var order = await _orderRepository.GetOrderWithItemsAndStoreAsync(orderCode);
		//	if (order == null) return;

		//	var apiKey = _configuration["Infobip:ApiKey"];
		//	var baseUrl = _configuration["Infobip:BaseUrl"];
		//	var sender = _configuration["Infobip:Sender"];

		//	var sb = new StringBuilder();
		//	sb.AppendLine($"ToyShelf: Thanh toan thanh cong #{orderCode}");

		//	foreach (var item in order.OrderItems)
		//	{
		//		var itemTotal = item.Price * item.Quantity;
		//		sb.AppendLine($"- {item.Quantity}x {item.ProductColor?.Sku}: {itemTotal:N0}d");
		//	}
		//	sb.AppendLine($"Tong: {order.TotalAmount:N0}d");
		//	sb.Append("Cam on ban!");

		//	var messageContent = sb.ToString();

		//	var finalPhone = phoneNumber.Replace(" ", "").Replace("-", "").Trim();

		//	if (finalPhone.StartsWith("0"))
		//	{
		//		finalPhone = "84" + finalPhone.Substring(1);
		//	}
		//	else if (finalPhone.StartsWith("+84"))
		//	{
		//		finalPhone = finalPhone.Substring(1);
		//	}
		//	else if (!finalPhone.StartsWith("84"))
		//	{
		//		finalPhone = "84" + finalPhone;
		//	}

		//	// 5. Gửi qua Infobip bằng HttpClient
		//	var client = _httpClientFactory.CreateClient();
		//	client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"App {apiKey}");

		//	var payload = new
		//	{
		//		messages = new[]
		//		{
		//	new {
		//		from = sender,
		//		destinations = new[] { new { to = finalPhone } },
		//		text = messageContent
		//	}
		//}
		//	};

		//	try
		//	{
		//		var response = await client.PostAsJsonAsync($"{baseUrl}/sms/2/text/advanced", payload);

		//		if (response.IsSuccessStatusCode)
		//		{
		//			Console.ForegroundColor = ConsoleColor.Green;
		//			Console.WriteLine($"[Infobip] SMS sent successfully for Order #{orderCode}");
		//			Console.ResetColor();
		//		}
		//		else
		//		{
		//			var errorDetail = await response.Content.ReadAsStringAsync();
		//			throw new Exception($"Infobip Error: {errorDetail}");
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.ForegroundColor = ConsoleColor.Red;
		//		Console.WriteLine($"[SMS ERROR] {ex.Message}");
		//		Console.ResetColor();
		//		throw;
		//	}
		//}

		public async Task SendPaymentSuccessSmsAsync(string phoneNumber, long orderCode)
		{
			var order = await _orderRepository.GetOrderWithDetailsByCodeAsync(orderCode);
			if (order == null) return;


			var apiKey = _configuration["eSms:ApiKey"];
			var secretKey = _configuration["eSms:SecretKey"];


			//var sb = new StringBuilder();
			//sb.AppendLine($"ToyShelf: Ma Xac Nhan cua ban la #{orderCode}");
			//foreach (var item in order.OrderItems)
			//{
			//	var itemTotal = item.Price * item.Quantity;
			//	sb.AppendLine($"- {item.Quantity}x {item.ProductColor.Sku}: {itemTotal:N0}");
			//}
			//sb.AppendLine($"Tong: {order.TotalAmount:N0}");
			//var messageContent = sb.ToString();
			//var messageContent = $" #{orderCode} cua ban da duoc he thong ghi nhan ";

			var messageContent = "Ma xac nhan cua ban la 123456. Khong chia se ma nay.";
			var targetPhone = phoneNumber.Replace(" ", "").Replace("-", "").Trim();
			if (targetPhone.StartsWith("+84")) targetPhone = "0" + targetPhone.Substring(3);
			if (targetPhone.StartsWith("84")) targetPhone = "0" + targetPhone.Substring(2);

			var client = _httpClientFactory.CreateClient();


			var url = $"http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone={targetPhone}&Content={Uri.EscapeDataString(messageContent)}&ApiKey={apiKey}&SecretKey={secretKey}&SmsType=8";

			try
			{
				var response = await client.GetAsync(url);
				var resultString = await response.Content.ReadAsStringAsync();

				// eSMS trả về chuỗi JSON, nếu thành công thì "CodeResult":"100"
				if (response.IsSuccessStatusCode && resultString.Contains("\"CodeResult\":\"100\""))
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"[eSMS] Gửi thành công cho #{orderCode}");
					Console.ResetColor();
				}
				else
				{
					throw new Exception($"eSMS Error: {resultString}");
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

