using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.Common.Extensions;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.User.Request;
using ToyShelf.Application.Models.User.Response;

namespace ToyShelf.API.Controllers
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
		[HttpGet]
		public async Task<ActionResult<BaseResponse<List<UserProfileResponse>>>> GetUsers(
			[FromQuery] bool? isActive
		)
		{
			var rs = await _userService.GetUsersAsync(isActive);
			return BaseResponse<List<UserProfileResponse>>.Ok(rs, "Get users successfully");
		}

		[HttpGet("profile")]
		[Authorize(Roles = "PartnerAdmin,Partner,Admin,Customer")]
		public async Task<ActionResult<BaseResponse<UserProfileResponse>>> GetProfile([FromServices] ICurrentUser currentUser)
		{
			var rs = await _userService.GetProfileByUserIdAsync(currentUser.UserId);
			return BaseResponse<UserProfileResponse>.Ok(rs, "Get user profile successfully");
		}

		// ===== UPDATE =====

		// Update user by admin
		[HttpPut("{userId}/by-admin")]
		[Authorize(Roles = "PartnerAdmin,Admin")]
		public async Task<ActionResult<BaseResponse<UserProfileResponse>>> Update(
			Guid userId,
			[FromBody] UpdateUserRequest request)
		{
			var rs = await _userService.UpdateUserAsync(userId, request);
			return BaseResponse<UserProfileResponse>.Ok(rs, "User updated successfully");
		}

		[HttpPut]
		[Authorize(Roles = "PartnerAdmin,Partner,Admin,Customer")]
		public async Task<ActionResult<BaseResponse<UserProfileResponse>>> UpdateMe(
		[FromBody] UpdateUserRequest request,
		[FromServices] ICurrentUser currentUser)
		{
			var userId = User.GetUserId();
			var rs = await _userService.UpdateUserAsync(currentUser.UserId, request);
			return BaseResponse<UserProfileResponse>.Ok(rs, "Profile updated successfully");
		}


		// ================= DISABLE / RESTORE =================

		[HttpPatch("{userId}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid userId)
		{
			await _userService.DisableUserAsync(userId);
			return ActionResponse.Ok("User disabled successfully");
		}

		[HttpPatch("{userId}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid userId)
		{
			await _userService.RestoreUserAsync(userId);
			return ActionResponse.Ok("User restored successfully");
		}
	}
}
