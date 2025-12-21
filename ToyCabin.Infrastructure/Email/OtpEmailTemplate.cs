using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Infrastructure.Email
{
	internal static class OtpEmailTemplate
	{
		public static string Build(
			string otp,
			OtpPurpose purpose,
			DateTime expiredAt,
			string? fullName)
		{
			var title = purpose switch
			{
				OtpPurpose.ACTIVATE_ACCOUNT => "Activate your account",
				OtpPurpose.RESET_PASSWORD => "Reset your password",
				OtpPurpose.SET_LOCAL_PASSWORD => "Set your password",
				_ => "Verification code"
			};

			var greeting = string.IsNullOrWhiteSpace(fullName)
				? "Hello,"
				: $"Hello <b>{fullName}</b>,";

			return $@"
				<!DOCTYPE html>
				<html>
				<head>
					<meta charset='UTF-8'>
				</head>
				<body style='font-family: Arial, sans-serif; background:#f6f6f6; padding:20px;'>
					<div style='max-width:520px;margin:auto;background:#ffffff;border-radius:8px;padding:24px'>
						<h2 style='color:#2d3748'>{title}</h2>
						<p>{greeting}</p>

						<p>
							Please use the following verification code:
						</p>

						<div style='
							font-size:28px;
							letter-spacing:4px;
							font-weight:bold;
							background:#edf2f7;
							padding:12px;
							text-align:center;
							border-radius:6px;
							margin:20px 0'>
							{otp}
						</div>

						<p>
							This code will expire at
							<b>{expiredAt:HH:mm dd/MM/yyyy} (GMT+7)</b>.
						</p>

						<p style='color:#718096;font-size:13px'>
							If you did not request this action, please ignore this email.
						</p>

						<hr style='margin:24px 0'/>

						<p style='font-size:12px;color:#a0aec0'>
							© ToyCabin. All rights reserved.
						</p>
					</div>
				</body>
				</html>";
		}
	}
}
