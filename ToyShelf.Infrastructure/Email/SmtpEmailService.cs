using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Notifications;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Common.Time;

namespace ToyShelf.Infrastructure.Email
{
	public class SmtpEmailService : IEmailService
	{
		private readonly EmailOptions _options;
		private readonly IOrderRepository _orderRepository;

		public SmtpEmailService(IOptions<EmailOptions> options, IOrderRepository orderRepository)
		{
			_options = options.Value;
			_orderRepository = orderRepository;
		}

		public async Task SendOtpAsync(
			string toEmail,
			string otpCode,
			OtpPurpose purpose,
			DateTime expiredAt,
			string? fullName = null)
		{
			var subject = purpose switch
			{
				OtpPurpose.ACTIVATE_ACCOUNT => "Activate your ToyCabin account",
				OtpPurpose.RESET_PASSWORD => "Reset your ToyCabin password",
				OtpPurpose.SET_LOCAL_PASSWORD => "Set your ToyCabin password",
				_ => "ToyCabin verification code"
			};

			var vietnamTimeZone = VietnamTimeZoneResolver.VietnamTimeZone;
			var expiredAtVn = TimeZoneInfo.ConvertTimeFromUtc(expiredAt, vietnamTimeZone);

			var body = OtpEmailTemplate.Build(
				otpCode,
				purpose,
				expiredAtVn,
				fullName);

			var mail = new MailMessage
			{
				From = new MailAddress(
					_options.SenderEmail,
					_options.SenderName),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			mail.To.Add(toEmail);

			using var smtp = new SmtpClient(
				_options.SmtpHost,
				_options.SmtpPort)
			{
				Credentials = new NetworkCredential(
					_options.Username,
					_options.Password),
				EnableSsl = _options.EnableSsl
			};

			await smtp.SendMailAsync(mail);
		}

		// Đổi tham số Order order thành long orderCode
		public async Task SendPaymentSuccessEmailAsync(string toEmail, long orderCode)
		{
			// 1. Phải móc DB ra lại cái Order kèm OrderItems
			var order = await _orderRepository.GetOrderWithDetailsByCodeAsync(orderCode);

			// Nếu lỡ không tìm thấy đơn hàng thì thoát luôn để khỏi báo lỗi
			if (order == null) return;

			var subject = $"[ToyShelf] Xác nhận thanh toán thành công đơn hàng #{order.OrderCode}";

			var body = BuildModernInvoiceTemplate( order);

			var mail = new MailMessage
			{
				From = new MailAddress(_options.SenderEmail, _options.SenderName),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			mail.To.Add(toEmail);

			using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
			{
				Credentials = new NetworkCredential(_options.Username, _options.Password),
				EnableSsl = _options.EnableSsl
			};

			await smtp.SendMailAsync(mail);
		}

		private string BuildModernInvoiceTemplate(Order order)
		{
			// 1. Dựng danh sách sản phẩm (Màu text xanh đậm #1a3a8f như ảnh)
			var itemsHtml = new StringBuilder();
			foreach (var item in order.OrderItems)
			{
				var productName = item.ProductColor?.Product?.Name ?? "Mô hình ToyShelf";
				var sku = item.ProductColor?.Sku ?? "KhongCoSKU";
				var itemTotal = item.Price * item.Quantity;

				itemsHtml.Append($@"
        <div style='border-bottom: 0.5px solid #eef1f8; padding: 12px 0;'>
            <table width='100%' border='0' cellpadding='0' cellspacing='0' style='width: 100%;'>
                <tr>
                    <td align='left' valign='top'>
                        <div style='font-size: 14px; font-weight: 500; color: #1a1a1a;'>{productName}</div>
                        <div style='font-size: 12px; color: #888; margin-top: 4px;'>SKU: {sku}</div>
                        <div style='font-size: 12px; color: #1a3a8f; margin-top: 4px;'>SL: {item.Quantity}</div>
                    </td>
                    
                    <td align='right' valign='top'>
                        <div style='font-size: 14px; font-weight: 500; color: #1a1a1a;'>{itemTotal:N0}₫</div>
                        <div style='font-size: 11px; color: #888; margin-top: 4px;'>{item.Price:N0}₫ / cái</div>
                    </td>
                </tr>
            </table>
        </div>");
			}

			// 2. Xử lý thời gian
			var vietnamTimeZone = VietnamTimeZoneResolver.VietnamTimeZone;
			var createdAtVn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedAt, vietnamTimeZone);

			// Format giống ảnh: "Thứ Tư, 22 tháng 4, 2026 - 00:53"
			var orderDate = createdAtVn.ToString("dddd, dd 'tháng' MM, yyyy - HH:mm", new System.Globalization.CultureInfo("vi-VN"));

			// 3. Render HTML - Y HỆT BẢN THIẾT KẾ CỦA SẾP
			var htmlContent = $$"""
    <!DOCTYPE html>
    <html lang="vi">
    <head>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
        <title>Hoá đơn ToyShelf</title>
    </head>
    <body style="margin: 0; padding: 2rem 1rem; background: #f4f6fb; font-family: Arial, sans-serif;">
      
    <div style="max-width: 520px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; border: 0.5px solid #e0e4ef;">
      
        <div style="background: #1a3a8f; padding: 28px 32px 28px; text-align: center;">
            <span style="font-size: 26px; font-weight: 700; color: #ffffff; letter-spacing: 0.5px;">ToyShelf</span>
        </div>
      
        <div style="padding: 20px 32px 0;">
            <div style="margin-bottom: 4px;">
                <span style="font-size: 17px; font-weight: 500; color: #1a1a1a;">Đơn hàng <span style="color: #1a3a8f;">#{{order.OrderCode}}</span></span>
            </div>
            <div style="font-size: 12px; color: #888; margin-bottom: 16px; text-transform: capitalize;">{{orderDate}}</div>
        </div>
      
        <div style="margin: 0 32px; border-top: 0.5px dashed #c5cfe0;"></div>
      
        <div style="padding: 16px 32px 0;">
            <div style="font-size: 11px; font-weight: 500; color: #888; text-transform: uppercase; letter-spacing: 0.8px; margin-bottom: 10px;">Chi tiết đơn hàng</div>
            
            {{itemsHtml}}
            
        </div>
      
       <div style="margin: 0 32px; padding: 14px 0 0;">
        <div style="border-top: 1.5px solid #1a3a8f; margin-bottom: 12px;"></div>

        <table width="100%" border="0" cellpadding="0" cellspacing="0" style="width: 100%;">
            <tr>
                <td align="left" style="font-size: 16px; font-weight: 500; color: #1a1a1a;">
                    Tổng thanh toán
                </td>
                <td align="right" style="font-size: 20px; font-weight: 700; color: #1a3a8f;">
                    {{order.TotalAmount:N0}}₫
                </td>
            </tr>
        </table>

    </div>
      
        <div style="margin: 20px 32px 0; background: #f0f4ff; border-radius: 8px; padding: 14px 18px; text-align: center; border: 0.5px solid #c5d0f5;">
            <div style="font-size: 15px; font-weight: 500; color: #1a3a8f; margin-bottom: 4px;">Cảm ơn bạn đã ủng hộ!</div>
            <div style="font-size: 12px; color: #5a6abf;">Hẹn gặp lại bạn lần sau tại ToyShelf 🎉</div>
        </div>
      
        <div style="padding: 18px 32px 24px; text-align: center; margin-top: 8px;">
            <div style="font-size: 11px; color: #999;">Đây là hoá đơn điện tử được gửi tự động.</div>
            <div style="font-size: 11px; color: #999; margin-top: 2px;">Vui lòng không trả lời email này.</div>
            <div style="margin-top: 10px; font-size: 11px; color: #8a96c0;">© 2026 ToyShelf. All rights reserved.</div>
        </div>
      
    </div>
    </body>
    </html>
    """;

			return htmlContent;
		}



	}
}