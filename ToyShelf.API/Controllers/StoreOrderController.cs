using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.StoreOrder.Request;
using ToyShelf.Application.Models.StoreOrder.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StoreOrderController : ControllerBase
	{
		private readonly IStoreOrderService _storeOrderService;

		public StoreOrderController(IStoreOrderService storeOrderService)
		{
			_storeOrderService = storeOrderService;
		}

		// ================= CREATE =================
		[HttpPost]
		public async Task<BaseResponse<StoreOrderResponse>> Create(
			[FromBody] CreateStoreOrderRequest request)
		{
			var result = await _storeOrderService.CreateAsync(request);

			return BaseResponse<StoreOrderResponse>
				.Ok(result, "Store order created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<StoreOrderResponse>>> GetAll(StoreOrderStatus status)
		{
			var result = await _storeOrderService.GetAllAsync(status);

			return BaseResponse<IEnumerable<StoreOrderResponse>>
				.Ok(result, "Store orders retrieved successfully");
		}

		[HttpGet("{id}")]
		public async Task<BaseResponse<StoreOrderResponse>> GetById(Guid id)
		{
			var result = await _storeOrderService.GetByIdAsync(id);

			return BaseResponse<StoreOrderResponse>
				.Ok(result, "Store order retrieved successfully");
		}

		// ================= APPROVE =================
		[HttpPatch("{id}/approve")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Approve(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _storeOrderService.ApproveAsync(id, currentUser);

			return ActionResponse.Ok("Store order approved successfully");
		}

		// ================= REJECT =================
		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Reject(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _storeOrderService.RejectAsync(id, currentUser);

			return ActionResponse.Ok("Store order rejected successfully");
		}
	}
}
