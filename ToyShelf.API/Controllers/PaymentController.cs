using Microsoft.AspNetCore.Mvc;
using PayOS;
using PayOS.Models.Webhooks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Order;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly IOrderService _orderService;
		private readonly PayOSClient _payOSClient;

		public PaymentController(IOrderService orderService, PayOSClient payOSClient)
		{
			_orderService = orderService;
			_payOSClient = payOSClient;
		}

		/// <summary>
		/// Luồng: Tạo Order trong DB -> Tạo link PayOS -> Trả link về cho Frontend
		/// </summary>
		[HttpPost("checkout")]
		public async Task<IActionResult> Checkout([FromBody] CreateOrderRequest request)
		{
			try
			{
				// Gọi OrderService để lưu DB (CREATED) và lấy link thanh toán
				// Hàm này bạn đã viết, bao gồm việc sinh OrderCode và gọi PayOSPaymentService
				string checkoutUrl = await _orderService.CreateOrderAndGetPaymentLinkAsync(request);

				return Ok(new { checkoutUrl });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		/// <summary>
		/// Webhook: PayOS gọi vào đây khi thanh toán thành công
		/// Luồng: Xác thực -> HandlePaymentSuccess (PAID + Tính hoa hồng)
		/// </summary>
		[HttpPost("webhook")]
		public async Task<IActionResult> ReceiveWebhook([FromBody] Webhook body)
		{
			try
			{
				// 1. Xác thực chữ ký. Nếu sai ChecksumKey nó sẽ văng lỗi ngay đây.
				var verifiedData = await _payOSClient.Webhooks.VerifyAsync(body);

				if (verifiedData != null)
				{
					try
					{
					
						await _orderService.HandlePaymentSuccessAsync(verifiedData.OrderCode);
					}
					catch (Exception dbEx)
					{
						
						Console.WriteLine($"[Cảnh báo DB]: {dbEx.Message}");
					}
				}

				return Ok(new { message = "Webhook processed successfully" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = "Invalid Signature: " + ex.Message });
			}
		}

	}
}
