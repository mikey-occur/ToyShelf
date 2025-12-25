using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.Common.Extensions;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Account.Request;
using ToyCabin.Application.Models.Account.Response;

namespace ToyCabin.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IAccountService _accountService;

		public AccountController(IAccountService accountService)
		{
			_accountService = accountService;
		}

		// ================= FLOW ACTIVATE =================
		[HttpPost("activate/internal")]
		public async Task<ActionResult<BaseResponse<CreateAccountResponse>>> CreateAccount([FromBody] CreateAccountRequest request)
		{
			var rs = await _accountService.CreateAccountAsync(request);
			return BaseResponse<CreateAccountResponse>.Ok(rs, "Account created successfully");
		}

		[HttpPost("activate/request")]
		public async Task<ActionResult<BaseResponse<ActivationOtpResponse>>> RequestActivateAccount([FromQuery] string email)
		{
			var rs = await _accountService.RequestActivateAccountAsync(email);
			return BaseResponse<ActivationOtpResponse>.Ok(rs, "Activation OTP sent to email");
		}

		[HttpPost("activate/confirm")]
		public async Task<ActionResult<BaseResponse<ActivateAccountResponse>>> ActivateAccount([FromBody] ActivateAccountRequest request)
		{
			var rs = await _accountService.ActivateAccountAndSetPasswordAsync(request);
			return BaseResponse<ActivateAccountResponse>.Ok(rs, "Account activated successfully");
		}

		// ================= FLOW LOCAL =================
		[HttpPost("register")]
		public async Task<ActionResult<BaseResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request)
		{
			var rs = await _accountService.RegisterLocalAsync(request);
			return BaseResponse<RegisterResponse>.Ok(rs, "User registered successfully");
		}

		[HttpPost("login")]
		public async Task<ActionResult<BaseResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
		{
			var rs = await _accountService.LoginLocalAsync(request);
			return BaseResponse<LoginResponse>.Ok(rs, "User logged in successfully");
		}

		// ================= FLOW LOGIN GG =================
		[HttpPost("login-google")]
		public async Task<ActionResult<BaseResponse<LoginResponse>>> LoginGoogle([FromBody] GoogleLoginRequest request)
		{
			var rs = await _accountService.LoginGoogleAsync(request.IdToken);
			return BaseResponse<LoginResponse>.Ok(rs, "User logged in with Google successfully");
		}

		[HttpPost("set-password")]
		public async Task<ActionResult<BaseResponse<SetLocalPasswordResponse>>> SetPassword([FromBody] SetLocalPasswordRequest request)
		{
			var accountId = User.GetAccountId(); // từ JWT
			var rs = await _accountService
				.SetLocalPasswordAsync(accountId, request);
			return BaseResponse<SetLocalPasswordResponse>
				.Ok(rs, "Local password created successfully");
		}

		// ================= FLOW FORGETPASSWORD =================
		[HttpPost("forgot-password/request")]
		public async Task<ActionResult<BaseResponse<ForgotPasswordOtpResponse>>> RequestForgotPassword(	[FromQuery] string email)
		{
			var rs = await _accountService.RequestForgotPasswordAsync(email);
			return BaseResponse<ForgotPasswordOtpResponse>
				.Ok(rs, "Reset password OTP sent to email");
		}

		[HttpPost("forgot-password/reset")]
		public async Task<ActionResult<BaseResponse<ResetPasswordResponse>>> ResetPassword(	[FromBody] ResetPasswordRequest request)
		{
			var rs = await _accountService.ResetPasswordAsync(request);
			return BaseResponse<ResetPasswordResponse>
				.Ok(rs, "Password reset successfully");
		}

	}
}
