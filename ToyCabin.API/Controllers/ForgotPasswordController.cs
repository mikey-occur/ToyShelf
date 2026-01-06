using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Account.Request;
using ToyCabin.Application.Models.Account.Response;

namespace ToyCabin.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ForgotPasswordController : ControllerBase
	{
		private readonly IForgotPasswordService _forgotPasswordService;
		public ForgotPasswordController(IForgotPasswordService forgotPasswordService)
		{
			_forgotPasswordService = forgotPasswordService;
		}

		// ================= FLOW FORGETPASSWORD =================
		[HttpPost("request")]
		public async Task<ActionResult<BaseResponse<ForgotPasswordOtpResponse>>> RequestForgotPassword([FromQuery] string email)
		{
			var rs = await _forgotPasswordService.RequestForgotPasswordAsync(email);
			return BaseResponse<ForgotPasswordOtpResponse>
				.Ok(rs, "Reset password OTP sent to email");
		}

		[HttpPost("reset")]
		public async Task<ActionResult<BaseResponse<ResetPasswordResponse>>> ResetPassword([FromBody] ResetPasswordRequest request)
		{
			var rs = await _forgotPasswordService.ResetPasswordAsync(request);
			return BaseResponse<ResetPasswordResponse>
				.Ok(rs, "Password reset successfully");
		}
	}
}
