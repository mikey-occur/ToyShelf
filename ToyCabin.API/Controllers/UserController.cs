using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.Common.Extensions;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.User.Request;
using ToyCabin.Application.Models.User.Response;

namespace ToyCabin.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;

		public UserController(IUserService userService)
		{
			_userService = userService;
		}

		// ===== GET =====

		[HttpGet("active")]
		public async Task<ActionResult<BaseResponse<List<UserProfileResponse>>>> GetActive()
		{
			var rs = await _userService.GetActiveUsersAsync();
			return BaseResponse<List<UserProfileResponse>>.Ok(rs, "Get active users successfully");
		}

		[HttpGet("inactive")]
		public async Task<ActionResult<BaseResponse<List<UserProfileResponse>>>> GetInactive()
		{
			var rs = await _userService.GetInactiveUsersAsync();
			return BaseResponse<List<UserProfileResponse>>.Ok(rs, "Get inactive users successfully");
		}

		[HttpGet("{userId}")]
		public async Task<ActionResult<BaseResponse<UserProfileResponse>>> GetProfile(Guid userId)
		{
			var rs = await _userService.GetProfileByUserIdAsync(userId);
			return BaseResponse<UserProfileResponse>.Ok(rs, "Get user profile successfully");
		}

		// ===== UPDATE =====

		// Update user by admin
		[HttpPut("{userId}")]
		public async Task<ActionResult<BaseResponse<UserProfileResponse>>> Update(
			Guid userId,
			[FromBody] UpdateUserRequest request)
		{
			var rs = await _userService.UpdateUserAsync(userId, request);
			return BaseResponse<UserProfileResponse>.Ok(rs, "User updated successfully");
		}

		[Authorize]
		[HttpPut("me")]
		public async Task<ActionResult<BaseResponse<UserProfileResponse>>> UpdateMe(
		[FromBody] UpdateUserRequest request)
		{
			var userId = User.GetUserId();
			var rs = await _userService.UpdateUserAsync(userId, request);
			return BaseResponse<UserProfileResponse>.Ok(rs, "Profile updated successfully");
		}


		// ===== DELETE / RESTORE =====

		[HttpPatch("{userId}/disable")]
		public async Task<ActionResult<BaseResponse<object>>> Disable(Guid userId)
		{
			await _userService.DisableUserAsync(userId);
			return BaseResponse<object>.Ok(null, "User disabled successfully");
		}

		[HttpPatch("{userId}/restore")]
		public async Task<ActionResult<BaseResponse<object>>> Restore(Guid userId)
		{
			await _userService.RestoreUserAsync(userId);
			return BaseResponse<object>.Ok(null, "User restored successfully");
		}
	}
}
