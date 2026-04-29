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

        public async Task SendSettlementPaymentEmailAsync(string toEmail, MonthlySettlement settlement, Partner partner, User partnerAdmin)
        {
            var subject = $"[ToyShelf] Xác nhận thanh toán hoa hồng - Kỳ {settlement.Month}/{settlement.Year}";

            // 2. Build the HTML Body
            var body = BuildSettlementInvoiceTemplate(settlement, partner, partnerAdmin);

            // 3. Construct the Email Message
            var mail = new MailMessage
            {
                From = new MailAddress(_options.SenderEmail, _options.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            // 4. Send using SMTP
            using var smtp = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
            {
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                EnableSsl = _options.EnableSsl
            };

            await smtp.SendMailAsync(mail);
        }

        private string BuildModernInvoiceTemplate(Order order)
        {
            // 1. Dựng danh sách sản phẩm (Màu text xanh đậm #1a3a8f)
            var itemsHtml = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                var productName = item.ProductColor?.Product?.Name ?? "Mô hình ToyShelf";
                var sku = item.ProductColor?.Sku ?? "KhongCoSKU";
                var itemTotal = item.Price * item.Quantity;

                itemsHtml.Append($@"    <div style='border-bottom: 0.5px solid #eef1f8; padding: 12px 0;'>        <table width='100%' border='0' cellpadding='0' cellspacing='0' style='width: 100%;'>            <tr>                <td align='left' valign='top'>                    <div style='font-size: 14px; font-weight: 500; color: #1a1a1a;'>{productName}</div>                    <div style='font-size: 12px; color: #888; margin-top: 4px;'>SKU: {sku}</div>                    <div style='font-size: 12px; color: #1a3a8f; margin-top: 4px;'>SL: {item.Quantity}</div>                </td>                                    <td align='right' valign='top'>                    <div style='font-size: 14px; font-weight: 500; color: #1a1a1a;'>{itemTotal:N0}₫</div>                    <div style='font-size: 11px; color: #888; margin-top: 4px;'>{item.Price:N0}₫ / cái</div>                </td>            </tr>        </table>    </div>");
            }

            // 2. Xử lý thời gian
            var vietnamTimeZone = VietnamTimeZoneResolver.VietnamTimeZone;
            var createdAtVn = TimeZoneInfo.ConvertTimeFromUtc(order.CreatedAt, vietnamTimeZone);
            var orderDate = createdAtVn.ToString("dddd, dd 'tháng' MM, yyyy - HH:mm", new System.Globalization.CultureInfo("vi-VN"));

            // 3. XỬ LÝ BANK REFERENCE 
            var bankRefHtml = string.IsNullOrEmpty(order.BankReference)
                ? "<div style='margin-bottom: 16px;'></div>" // Giữ nguyên khoảng cách padding cũ nếu trống
                : $@"<div style='margin-bottom: 16px; font-size: 12px; color: #888;'>Mã giao dịch: <span style='font-weight: 600; color: #1a1a1a;'>{order.BankReference}</span></div>";

            // 4. Render HTML
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
        <div style="font-size: 12px; color: #888; margin-bottom: 4px; text-transform: capitalize;">{{orderDate}}</div>
        {{bankRefHtml}}
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

        private string BuildSettlementInvoiceTemplate(MonthlySettlement settlement, Partner partner, User partnerAdmin)
        {
            // 1. Xử lý ngày giờ hiển thị
            var vietnamTimeZone = VietnamTimeZoneResolver.VietnamTimeZone;
            var paidAtVn = TimeZoneInfo.ConvertTimeFromUtc(settlement.PaidAt ?? DateTime.UtcNow, vietnamTimeZone);
            var paymentDate = paidAtVn.ToString("dddd, dd 'tháng' MM, yyyy", new System.Globalization.CultureInfo("vi-VN"));

            // 2. Xử lý nút Xem ảnh Biên lai (Chỉ hiện khi có link ảnh)
            var receiptHtml = string.IsNullOrEmpty(settlement.TransferReceiptUrl)
                ? ""
                : $@"
          <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""margin-top:16px;"">
            <tr>
              <td align=""center"">
                <a href=""{settlement.TransferReceiptUrl}"" target=""_blank"" style=""display:inline-block; font-family:Arial, Helvetica, sans-serif; font-size:12px; font-weight:700; color:#1a3a8f; text-decoration:none; border: 1.5px solid #1a3a8f; padding: 10px 24px; border-radius: 6px; letter-spacing: 0.5px;"">
                  📎 XEM ẢNH BIÊN LAI CHUYỂN KHOẢN
                </a>
              </td>
            </tr>
          </table>";

            // 3. Render HTML
            var htmlContent = $"""
                 <!DOCTYPE html>
                 <html lang="vi" xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office">
                 <head>
                   <meta charset="UTF-8">
                   <meta name="viewport" content="width=device-width, initial-scale=1.0">
                   <meta http-equiv="X-UA-Compatible" content="IE=edge">
                   <meta name="x-apple-disable-message-reformatting">
                   <title>Biên Lai Chuyển Khoản Hoa Hồng - ToyShelf</title>
                   </head>
                 <body style="margin:0;padding:0;background-color:#f4f6fb;font-family:Arial,Helvetica,sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;">

                   <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="background-color:#f4f6fb;margin:0;padding:0;">
                     <tr>
                       <td align="center" style="padding:32px 16px;">

                         <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="600" style="max-width:600px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(26,58,143,0.10);">

                           <tr>
                             <td style="background-color:#1a3a8f;padding:0;">
                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                 <tr>
                                   <td style="padding:28px 40px 24px 40px;">
                                     <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                       <tr>
                                         <td style="vertical-align:middle;">
                                           <table role="presentation" cellspacing="0" cellpadding="0" border="0">
                                             <tr>
                                               <td style="vertical-align:middle;">
                                                 <span style="font-family:Arial,Helvetica,sans-serif;font-size:22px;font-weight:700;color:#ffffff;letter-spacing:0.5px;">ToyShelf</span>
                                               </td>
                                             </tr>
                                           </table>
                                         </td>
                                       </tr>
                                     </table>
                                   </td>
                                 </tr>
                                 <tr>
                                   <td style="background-color:rgba(0,0,0,0.18);padding:16px 40px;">
                                     <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                       <tr>
                                         <td>
                                           <span style="font-family:Arial,Helvetica,sans-serif;font-size:18px;font-weight:700;color:#ffffff;letter-spacing:1px;text-transform:uppercase;">BIÊN LAI CHUYỂN KHOẢN</span>
                                           <span style="font-family:Arial,Helvetica,sans-serif;font-size:12px;color:rgba(255,255,255,0.65);display:block;margin-top:3px;letter-spacing:0.3px;">Thanh toán hoa hồng kỳ đối soát</span>
                                         </td>
                                         <td style="text-align:right;white-space:nowrap;">
                                           <span style="font-family:Arial,Helvetica,sans-serif;font-size:11px;color:rgba(255,255,255,0.55);">Ngày phát hành</span>
                                           <span style="font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#ffffff;display:block;font-weight:600;">{paymentDate}</span>
                                         </td>
                                       </tr>
                                     </table>
                                   </td>
                                 </tr>
                               </table>
                             </td>
                           </tr>

                           <tr>
                             <td style="padding:36px 40px 0 40px;">

                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                 <tr>
                                   <td style="padding-bottom:28px;border-bottom:1px solid #eaedf5;">
                                     <p style="margin:0 0 6px 0;font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#555555;">Kính gửi Đối tác,</p>
                                     <p style="margin:0 0 4px 0;font-family:Arial,Helvetica,sans-serif;font-size:20px;font-weight:700;color:#1a3a8f;">{partner.CompanyName}</p>
                                     <p style="margin:0;font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#777777;">
                                       Người nhận:&nbsp;
                                       <span style="color:#1a1a1a;font-weight:600;">{partnerAdmin.FullName}</span>
                                     </p>
                                     <p style="margin:12px 0 0 0;font-family:Arial,Helvetica,sans-serif;font-size:13.5px;color:#555555;line-height:1.7;">
                                       ToyShelf xin trân trọng thông báo đã hoàn tất giao dịch chuyển khoản hoa hồng kỳ đối soát&nbsp;<strong style="color:#1a3a8f;">Tháng {settlement.Month}/{settlement.Year}</strong>&nbsp;đến tài khoản ngân hàng đã đăng ký của quý đối tác.
                                     </p>
                                   </td>
                                 </tr>
                               </table>

                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin-top:28px;">
                                 <tr>
                                   <td>
                                     <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="background-color:#f0f4ff;border-radius:10px;border:1.5px solid #d0dbf5;overflow:hidden;">
                                       <tr>
                                         <td width="5" style="background-color:#1a3a8f;border-radius:10px 0 0 10px;">&nbsp;</td>
                                         <td style="padding:20px 24px;">
                                           <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                             <tr>
                                               <td width="44" style="vertical-align:top;padding-right:16px;">
                                                 <div style="width:44px;height:44px;background-color:#1a3a8f;border-radius:50%;text-align:center;line-height:44px;font-size:20px;display:inline-block;">🏦</div>
                                               </td>
                                               <td style="vertical-align:middle;">
                                                 <div style="margin-bottom: 12px;">
                                                   <span style="font-family:Arial,Helvetica,sans-serif;font-size:10px;color:#777777;letter-spacing:1.5px;text-transform:uppercase;display:block;margin-bottom:4px;">Tài khoản nhận tiền</span>
                                                   <span style="font-family:Arial,Helvetica,sans-serif;font-size:15px;font-weight:700;color:#1a3a8f;">{partner.BankName}</span>
                                                 </div>
                                                 <div>
                                                   <span style="font-family:Arial,Helvetica,sans-serif;font-size:10px;color:#777777;letter-spacing:1.5px;text-transform:uppercase;display:block;margin-bottom:4px;">Số tài khoản</span>
                                                   <span style="font-family:Arial,Helvetica,sans-serif;font-size:16px;font-weight:700;color:#1a1a1a;letter-spacing:1px;word-break:break-all;">{partner.BankAccountNumber}</span>
                                                 </div>
                                               </td>
                                             </tr>
                                           </table>
                                         </td>
                                       </tr>
                                     </table>
                                   </td>
                                 </tr>
                               </table>

                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin-top:28px;">
                                 <tr>
                                   <td>
                                     <p style="margin:0 0 14px 0;font-family:Arial,Helvetica,sans-serif;font-size:12px;font-weight:700;color:#999999;letter-spacing:2px;text-transform:uppercase;">Chi tiết giao dịch</p>
                                     <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="border:1px solid #eaedf5;border-radius:8px;overflow:hidden;">
                                       <tr style="background-color:#fafbfe;">
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#777777;border-bottom:1px solid #eaedf5;width:55%;">Kỳ đối soát</td>
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;font-weight:600;color:#1a1a1a;border-bottom:1px solid #eaedf5;text-align:right;">Tháng {settlement.Month}/{settlement.Year}</td>
                                       </tr>
                                       <tr style="background-color:#ffffff;">
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#777777;border-bottom:1px solid #eaedf5;">Tổng đơn hàng</td>
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;font-weight:600;color:#1a1a1a;border-bottom:1px solid #eaedf5;text-align:right;">{settlement.TotalItems} đơn</td>
                                       </tr>
                                       <tr style="background-color:#fafbfe;">
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#777777;border-bottom:1px solid #eaedf5;">Tổng doanh thu</td>
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;font-weight:600;color:#1a1a1a;border-bottom:1px solid #eaedf5;text-align:right;">{settlement.TotalSalesAmount:N0}₫</td>
                                       </tr>
                                       <tr style="background-color:#ffffff;">
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#777777;border-bottom:1px solid #eaedf5;">Tổng hoa hồng</td>
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;font-weight:600;color:#1a3a8f;border-bottom:1px solid #eaedf5;text-align:right;">{settlement.TotalCommissionAmount:N0}₫</td>
                                       </tr>
                                       <tr style="background-color:#fafbfe;">
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;color:#777777;">Khấu trừ</td>
                                         <td style="padding:13px 20px;font-family:Arial,Helvetica,sans-serif;font-size:13px;font-weight:600;color:#d0021b;text-align:right;">-{settlement.DeductionAmount:N0}₫</td>
                                       </tr>
                                     </table>
                                   </td>
                                 </tr>
                               </table>

                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin-top:24px;">
                                 <tr>
                                   <td style="background:linear-gradient(135deg, #1a3a8f 0%, #2352c8 100%);border-radius:10px;padding:26px 28px;">
                                     <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                       <tr>
                                         <td style="vertical-align:middle;">
                                           <span style="font-family:Arial,Helvetica,sans-serif;font-size:11px;color:rgba(255,255,255,0.65);letter-spacing:2px;text-transform:uppercase;display:block;margin-bottom:6px;">Số tiền thực nhận</span>
                                           <span style="font-family:Arial,Helvetica,sans-serif;font-size:28px;font-weight:700;color:#ffffff;letter-spacing:-0.5px;">{settlement.FinalAmount:N0}₫</span>
                                         </td>
                                         <td style="text-align:right;vertical-align:middle;">
                                           <div style="display:inline-block;background-color:rgba(255,255,255,0.15);border-radius:50%;width:52px;height:52px;text-align:center;line-height:52px;font-size:26px;">✓</div>
                                         </td>
                                       </tr>
                                     </table>
                                   </td>
                                 </tr>
                               </table>

                               {receiptHtml}

                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin-top:32px;">
                                 <tr>
                                   <td style="border-top:1px solid #eaedf5;"></td>
                                 </tr>
                               </table>

                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%" style="margin-top:28px;margin-bottom:36px;">
                                 <tr>
                                   <td align="center">
                                     <p style="margin:0 0 16px 0;font-family:Arial,Helvetica,sans-serif;font-size:13.5px;color:#555555;text-align:center;">Vui lòng đăng nhập vào hệ thống để xác nhận giao dịch và lưu trữ biên lai.</p>
                                     <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:0 auto;">
                                       <tr>
                                         <td style="border-radius:8px;background-color:#1a3a8f;text-align:center;">
                                           <a href="https://toy-shelf-management.vercel.app/" target="_blank" style="display:inline-block;background-color:#1a3a8f;color:#ffffff;font-family:Arial,Helvetica,sans-serif;font-size:14px;font-weight:700;letter-spacing:1px;text-decoration:none;padding:14px 40px;border-radius:8px;text-transform:uppercase;">ĐĂNG NHẬP ĐỂ XÁC NHẬN</a>
                                           </td>
                                       </tr>
                                     </table>
                                   </td>
                                 </tr>
                               </table>

                             </td>
                           </tr>

                           <tr>
                             <td style="background-color:#f4f6fb;border-top:1px solid #eaedf5;padding:24px 40px;border-radius:0 0 12px 12px;">
                               <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="100%">
                                 <tr>
                                   <td style="text-align:center;">
                                     <p style="margin:0 0 8px 0;font-family:Arial,Helvetica,sans-serif;font-size:13px;font-weight:700;color:#1a3a8f;">ToyShelf</p>
                                     <p style="margin:0 0 6px 0;font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#999999;line-height:1.6;">
                                       Cảm ơn quý đối tác đã đồng hành cùng ToyShelf.<br>
                                       Mọi thắc mắc vui lòng liên hệ <a href="mailto:support@toyshelf.vn" style="color:#1a3a8f;text-decoration:none;">support@toyshelf.vn</a>
                                     </p>
                                     <table role="presentation" cellspacing="0" cellpadding="0" border="0" width="80%" style="margin:12px auto;">
                                       <tr><td style="border-top:1px solid #dde2ee;"></td></tr>
                                     </table>
                                     <p style="margin:0;font-family:Arial,Helvetica,sans-serif;font-size:11px;color:#bbbbbb;font-style:italic;">
                                       Đây là email tự động được gửi bởi hệ thống ToyShelf. Vui lòng không reply trực tiếp email này.
                                     </p>
                                     <p style="margin:6px 0 0 0;font-family:Arial,Helvetica,sans-serif;font-size:10px;color:#cccccc;">
                                       © {DateTime.Now.Year} ToyShelf. All rights reserved.
                                     </p>
                                   </td>
                                 </tr>
                               </table>
                             </td>
                           </tr>

                         </table>
                       </td>
                     </tr>
                   </table>

                 </body>
                 </html>
                 """;

            return htmlContent;
        }
    }
}