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

				// 1. Xác thực chữ ký từ PayOS để đảm bảo an toàn, tránh tin tặc giả mạo
				var verifiedData = await _payOSClient.Webhooks.VerifyAsync(body);

				if (verifiedData != null)
				{
					
					await _orderService.HandlePaymentSuccessAsync(verifiedData.OrderCode);
				}

				return Ok(new { message = "Webhook processed successfully" });
			}
			catch (Exception ex)
			{
			
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("webhookt")]
		public async Task<IActionResult> ReceivetestWebhook([FromBody] Webhook body)
		{
			try
			{
				// Khi test Postman: Comment dòng Verify của SDK lại
				// var verifiedData = await _payOSClient.Webhooks.VerifyAsync(body);

				// Giả lập dữ liệu đã xác thực (Dùng đúng mã OrderCode trong DB của bạn)
				long testOrderCode = body.Data.OrderCode;

				// Gọi logic tính hoa hồng
				Guid? orderId = await _orderService.HandlePaymentSuccessAsync(testOrderCode);

				return Ok(new { message = "Gia lap Webhook thanh cong", orderId });
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}
