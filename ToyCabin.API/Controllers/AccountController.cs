using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Account.Request;

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

		// ================= ADMIN =================
		[HttpPost("admin")]
		public async Task<ActionResult<BaseResponse<object>>> CreateAccount(
			[FromBody] CreateAccountRequest request)
		{
			await _accountService.CreateAccountAsync(request);

			return Ok(BaseResponse<object>.Ok(
				data: null,
				message: "Account created successfully"
			));
		}

		// ================= ACTIVATE =================
		[HttpPost("activate/request")]
		public async Task<ActionResult<BaseResponse<object>>> RequestActivateAccount(
			[FromQuery] string email)
		{
			await _accountService.RequestActivateAccountAsync(email);

			return Ok(BaseResponse<object>.Ok(
				data: null,
				message: "Activation OTP sent to email"
			));
		}

		// ================= ACTIVATE =================
		[HttpPost("activate")]
		public async Task<ActionResult<BaseResponse<object>>> ActivateAccount(
			[FromBody] ActivateAccountRequest request)
		{
			await _accountService.ActivateAccountAndSetPasswordAsync(request);

			return Ok(BaseResponse<object>.Ok(
				data: null,
				message: "Account activated successfully"
			));
		}
	}
}
