using Microsoft.AspNetCore.Mvc;
using PayOS;
using PayOS.Models.Webhooks;
using ToyShelf.Application.Common;
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
				var result = await _orderService.CreateOrderAndGetPaymentLinkAsync(request);

				return Ok(new
				{
					orderCode = result.OrderCode,
					checkoutUrl = result.PaymentUrl
				});
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
				// 1. Xác thực chữ ký từ PayOS
				var verifiedData = await _payOSClient.Webhooks.VerifyAsync(body);

				if (verifiedData != null)
				{
					// 2. Chạy logic xử lý đơn hàng

					if (verifiedData.OrderCode == 123 || verifiedData.Description == "Ma test")
					{
						Console.WriteLine("[PAYOS] Đã chặn thành công cái request test ping!");
						return Ok(new { message = "Test webhook success" });
					}
					await _orderService.HandlePaymentSuccessAsync(verifiedData.OrderCode, verifiedData.Reference);
				}

				return Ok(new { message = "Webhook processed successfully" });
			}
			catch (Exception ex)
			{
				
				Console.WriteLine($"[WEBHOOK ERROR]: {ex.Message}\n{ex.StackTrace}");

				// Nếu lỗi có chữ "Signature" (Sai chữ ký) thì trả 400.
				if (ex.Message.Contains("Signature") || ex.Message.Contains("checksum"))
				{
					return BadRequest(new { message = "Invalid Signature" });
				}

				return StatusCode(500, new { message = "Internal server error. Please retry." });
			}
		}
		

		/// <summary>
		/// Dành cho hàm checkPayment trên Frontend: Kiểm tra trạng thái thanh toán từ Database
		/// GET: api/Payment/check-payment?orderCode=123456
		/// </summary>
		[HttpGet("check-payment")]
		public async Task<IActionResult> CheckPayment([FromQuery] long orderCode)
		{
			try
			{
				// Gọi Service tìm đơn hàng trong DB bằng OrderCode
				var order = await _orderService.GetOrderDetailsAsync(orderCode);

				if (order == null)
				{
					return NotFound(new { message = "Không tìm thấy đơn hàng với mã này!" });
				}

				// Trả về status hiện tại đang lưu trong bảng Orders (VD: PENDING, PAID, CANCELLED)
				return Ok(new { status = order.Status });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = $"Lỗi khi kiểm tra DB: {ex.Message}" });
			}
		}

		/// <summary>
		/// [TEST ONLY] Cửa sau giả lập PayOS gọi Webhook thành công.
		/// Giúp test nhanh logic tính hoa hồng mà không cần thanh toán thật.
		/// </summary>
		[HttpPost("mock-webhook/{orderCode}")]
		public async Task<IActionResult> MockPaymentSuccess(long orderCode, string? bankReference = null)
		{
			try
			{
				// Bỏ qua toàn bộ bước xác thực chữ ký của PayOS.
				// Đâm thẳng vào hàm xử lý cập nhật trạng thái PAID và TÍNH HOA HỒNG!
				await _orderService.HandlePaymentSuccessAsync(orderCode, bankReference);

				return Ok(new
				{
					success = true,
					message = $"[MOCK] Đã giả lập thanh toán thành công cho đơn {orderCode} và chạy xong logic tính tiền!"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = $"Lỗi khi chạy logic tính tiền: {ex.Message}",
					detail = ex.StackTrace
				});
			}
		}

	
	}
}
