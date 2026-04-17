using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.StoreOrder.Request;
using ToyShelf.Application.Models.StoreOrder.Response;
using ToyShelf.Application.Models.Warehouse.Response;
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
		[Authorize(Roles = "Partner")]
		public async Task<BaseResponse<StoreOrderResponse>> Create(
			[FromBody] CreateStoreOrderRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _storeOrderService.CreateAsync(request, currentUser);

			return BaseResponse<StoreOrderResponse>
				.Ok(result, "Store order created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<StoreOrderResponse>>> GetAll(StoreOrderStatus? status)
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

		[HttpPatch("{id}/partner-approve")]
		[Authorize(Roles = "PartnerAdmin")]
		public async Task<ActionResult<ActionResponse>> PartnerApprove(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _storeOrderService.PartnerAdminApproveAsync(id, currentUser);
			return ActionResponse.Ok("Partner has approved the order. Waiting for Admin final approval.");
		}

		[HttpPatch("{id}/admin-approve")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> AdminApprove(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _storeOrderService.AdminApproveAsync(id, currentUser);
			return ActionResponse.Ok("Store order approved by Admin successfully.");
		}

		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Admin,PartnerAdmin")]
		public async Task<ActionResult<ActionResponse>> Reject(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _storeOrderService.RejectAsync(id, currentUser);

			return ActionResponse.Ok("Store order rejected successfully");
		}

		// ================= MATCH WAREHOUSE =================
		[HttpGet("{id}/available-warehouses")]
		//[Authorize(Roles = "Admin")]
		public async Task<BaseResponse<List<WarehouseMatchResponse>>> GetAvailableWarehouses(Guid id)
		{
			var result = await _storeOrderService.GetAvailableWarehousesAsync(id);

			return BaseResponse<List<WarehouseMatchResponse>>
				.Ok(result, "Available warehouses retrieved successfully");
		}
	}
}
