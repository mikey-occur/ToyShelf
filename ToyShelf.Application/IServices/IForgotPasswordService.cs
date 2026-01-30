using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Account.Request;
using ToyShelf.Application.Models.Account.Response;

namespace ToyShelf.Application.IServices
{
	public interface IForgotPasswordService
	{
		// ===== FLOW FORGOT PASSWORD =====
		Task<ForgotPasswordOtpResponse> RequestForgotPasswordAsync(string email);
		Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
	}
}
