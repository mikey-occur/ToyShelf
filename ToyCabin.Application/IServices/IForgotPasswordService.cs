using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Account.Request;
using ToyCabin.Application.Models.Account.Response;

namespace ToyCabin.Application.IServices
{
	public interface IForgotPasswordService
	{
		// ===== FLOW FORGOT PASSWORD =====
		Task<ForgotPasswordOtpResponse> RequestForgotPasswordAsync(string email);
		Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
	}
}
