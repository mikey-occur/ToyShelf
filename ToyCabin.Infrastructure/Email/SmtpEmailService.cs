using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Notifications;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Infrastructure.Email
{
	public class SmtpEmailService : IEmailService
	{
		private readonly EmailOptions _options;

		public SmtpEmailService(IOptions<EmailOptions> options)
		{
			_options = options.Value;
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

			var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
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
	}
}
