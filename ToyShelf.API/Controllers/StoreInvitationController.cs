using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.UserStore.Request;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StoreInvitationController : ControllerBase
	{
		private readonly IStoreInvitationService _storeInvitationService;

		public StoreInvitationController(IStoreInvitationService storeInvitationService)
		{
			_storeInvitationService = storeInvitationService;
		}

		[HttpPost("invite")]
		[Authorize(Roles = "PartnerAdmin,Partner")]
		public async Task<ActionResult<BaseResponse<bool>>> InviteUser(
			[FromBody] InviteUserToStoreRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _storeInvitationService.InviteUserAsync(
				request,
				currentUser
			);

			return BaseResponse<bool>.Ok(result, "Invitation sent successfully");
		}

		[HttpPost("{invitationId:guid}/accept")]
		[Authorize]
		public async Task<ActionResult<BaseResponse<bool>>> AcceptInvitation(
			Guid invitationId,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _storeInvitationService.AcceptInvitationAsync(
				invitationId,
				currentUser.UserId
			);

			return BaseResponse<bool>.Ok(result, "Invitation accepted");
		}

		[HttpPost("{invitationId:guid}/reject")]
		[Authorize]
		public async Task<ActionResult<BaseResponse<bool>>> RejectInvitation(
			Guid invitationId,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _storeInvitationService.RejectInvitationAsync(
				invitationId,
				currentUser.UserId
			);
			return BaseResponse<bool>.Ok(result, "Invitation rejected");
		}

	}
}
