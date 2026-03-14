using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.StoreOrder.Request;
using ToyShelf.Application.Models.StoreOrder.Response;

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
		public async Task<ActionResult<BaseResponse<StoreOrderResponse>>> Create(
			[FromBody] CreateStoreOrderRequest request)
		{
			var result = await _storeOrderService.CreateAsync(request);

			return BaseResponse<StoreOrderResponse>
				.Ok(result, "Store order created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<ActionResult<BaseResponse<IEnumerable<StoreOrderResponse>>>> GetAll()
		{
			var result = await _storeOrderService.GetAllAsync();

			return BaseResponse<IEnumerable<StoreOrderResponse>>
				.Ok(result, "Store orders retrieved successfully");
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<StoreOrderResponse>>> GetById(Guid id)
		{
			var result = await _storeOrderService.GetByIdAsync(id);

			return BaseResponse<StoreOrderResponse>
				.Ok(result, "Store order retrieved successfully");
		}

		// ================= APPROVE =================
		[HttpPatch("{id}/approve")]
		public async Task<ActionResult<ActionResponse>> Approve(Guid id)
		{
			await _storeOrderService.ApproveAsync(id);

			return ActionResponse.Ok("Store order approved successfully");
		}

		// ================= REJECT =================
		[HttpPatch("{id}/reject")]
		public async Task<ActionResult<ActionResponse>> Reject(Guid id)
		{
			await _storeOrderService.RejectAsync(id);

			return ActionResponse.Ok("Store order rejected successfully");
		}
	}
}
