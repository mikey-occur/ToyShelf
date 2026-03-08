using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Domain.Entities;
using PayOSItem = PayOS.Models.V2.PaymentRequests.PaymentLinkItem;
using PayOSRequest = PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest;
using PayOSResponse = PayOS.Models.V2.PaymentRequests.CreatePaymentLinkResponse;

namespace ToyShelf.Infrastructure.Common.Payment
{
	public class PayOSPaymentService : IPaymentService
	{
		private readonly PayOSClient _client;
		private readonly IConfiguration _configuration;
		public PayOSPaymentService(PayOSClient client, IConfiguration configuration)
		{
			_client = client;
			_configuration = configuration;
		}

		public async Task<string> CreatePaymentLink(Order order)
		{

			var items = order.OrderItems.Select(x => new PayOSItem
			{
				Name = "Sản phẩm ToyShelf",
				Quantity = x.Quantity,
				Price = (int)x.Price
			}).ToList();

			string returnUrl = _configuration["PayOS:ReturnUrl"]
					   ?? throw new Exception("Not found return url in appsettings.json");

			string cancelUrl = _configuration["PayOS:CancelUrl"]
							   ?? throw new Exception("Not Found CancelUrl trong appsettings.json");
			var paymentRequest = new PayOSRequest
			{
				OrderCode = order.OrderCode,
				Amount = (int)order.TotalAmount,
				Description = $"Thanh toan don {order.OrderCode}",
				Items = items,
				ReturnUrl = returnUrl,
				CancelUrl = cancelUrl
			};

			// 3. Tạo link thông qua PaymentRequests.CreateAsync()
			var paymentLinkResponse = await _client.PaymentRequests.CreateAsync(paymentRequest);

			// Trả về CheckoutUrl từ response
			return paymentLinkResponse.CheckoutUrl;
		}
	}
}