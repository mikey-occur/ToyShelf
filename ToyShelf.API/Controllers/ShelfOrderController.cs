using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.ShelfOrder.Request;
using ToyShelf.Application.Models.ShelfOrder.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShelfOrderController : ControllerBase
	{
		private readonly IShelfOrderService _shelfOrderService;

		public ShelfOrderController(IShelfOrderService shelfOrderService)
		{
			_shelfOrderService = shelfOrderService;
		}

		// ================= CREATE =================
		[HttpPost]
		[Authorize(Roles = "Partner")]
		public async Task<BaseResponse<ShelfOrderResponse>> Create(
			[FromBody] CreateShelfOrderRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _shelfOrderService.CreateAsync(request, currentUser);

			return BaseResponse<ShelfOrderResponse>
				.Ok(result, "Shelf order created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ShelfOrderResponse>>> GetAll(ShelfOrderStatus? status)
		{
			var result = await _shelfOrderService.GetAllAsync(status);

			return BaseResponse<IEnumerable<ShelfOrderResponse>>
				.Ok(result, "Shelf orders retrieved successfully");
		}

		[HttpGet("{id}")]
		public async Task<BaseResponse<ShelfOrderResponse>> GetById(Guid id)
		{
			var result = await _shelfOrderService.GetByIdAsync(id);

			return BaseResponse<ShelfOrderResponse>
				.Ok(result, "Shelf order retrieved successfully");
		}

		// ================= APPROVE =================
		[HttpPatch("{id}/approve")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Approve(
			Guid id,
			[FromServices] ICurrentUser currentUser)
		{
			await _shelfOrderService.ApproveAsync(id, currentUser);

			return ActionResponse.Ok("Shelf order approved successfully");
		}

		// ================= REJECT =================
		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Reject(
			Guid id,
			[FromBody] string? adminNote,
			[FromServices] ICurrentUser currentUser)
		{
			await _shelfOrderService.RejectAsync(id, adminNote, currentUser);

			return ActionResponse.Ok("Shelf order rejected successfully");
		}

		// ================= FULFILL =================
		//[HttpPatch("{id}/fulfill")]
		//[Authorize(Roles = "Admin")]
		//public async Task<ActionResult<ActionResponse>> Fulfill(Guid id)
		//{
		//	await _shelfOrderService.FulfillAsync(id);

		//	return ActionResponse.Ok("Shelf order fulfilled successfully");
		//}
	}
}
