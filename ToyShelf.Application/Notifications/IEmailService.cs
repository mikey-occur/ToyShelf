using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Notifications
{
	public interface IEmailService
	{
		Task SendOtpAsync(
			string toEmail,
			string otpCode,
			OtpPurpose purpose,
			DateTime expiredAt,
			string? fullName = null);
	}
}
