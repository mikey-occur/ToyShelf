using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.ShelfOrder.Request;
using ToyShelf.Application.Models.ShelfOrder.Response;
using ToyShelf.Application.Models.StoreOrder.Response;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Application.Services;
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
		public async Task<BaseResponse<IEnumerable<ShelfOrderResponse>>> GetAll(ShelfOrderStatus? status, [FromQuery] Guid? storeId,
	[FromQuery] Guid? partnerId)
		{
			var result = await _shelfOrderService.GetAllAsync(status, storeId, partnerId);

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

		[HttpPatch("{id}/partner-approve")]
		[Authorize(Roles = "PartnerAdmin")]
		public async Task<ActionResult<ActionResponse>> PartnerApprove(
			Guid id,
			[FromServices] ICurrentUser currentUser)
		{
			await _shelfOrderService.PartnerAdminApproveAsync(id, currentUser);

			return ActionResponse.Ok("Shelf order has been approved by Partner Admin. Pending final Admin approval.");
		}

		[HttpPatch("{id}/admin-approve")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Approve(
			Guid id,
			[FromServices] ICurrentUser currentUser)
		{
			await _shelfOrderService.ApproveAsync(id, currentUser);

			return ActionResponse.Ok("Shelf order fully approved by Admin.");
		}

		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Admin,PartnerAdmin")]
		public async Task<ActionResult<ActionResponse>> Reject(
			Guid id,
			[FromBody] string? adminNote,
			[FromServices] ICurrentUser currentUser)
		{
			await _shelfOrderService.RejectAsync(id, adminNote, currentUser);

			return ActionResponse.Ok("Shelf order has been rejected.");
		}

		// ================= AVAILABLE WAREHOUSES =================
		[HttpGet("{id}/available-warehouses")]
		[Authorize(Roles = "Admin,Partner")]
		public async Task<BaseResponse<List<WarehouseMatchShelfResponse>>> GetAvailableWarehouses(Guid id)
		{
			var result = await _shelfOrderService.GetAvailableWarehousesForShelfOrder(id);

			return BaseResponse<List<WarehouseMatchShelfResponse>>
				.Ok(result, "Available warehouses retrieved successfully");
		}

		[HttpGet("by-partner/{partnerId}")]
		[Authorize(Roles = "Admin,PartnerAdmin")]
		public async Task<BaseResponse<IEnumerable<ShelfOrderResponse>>> GetByPartner(
			Guid partnerId,
			[FromQuery] ShelfOrderStatus? status)
		{
			var result = await _shelfOrderService.GetByPartnerAsync(partnerId, status);

			return BaseResponse<IEnumerable<ShelfOrderResponse>>
				.Ok(result, "Partner shelf orders retrieved successfully");
		}
	}
}
