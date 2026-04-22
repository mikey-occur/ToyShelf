using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.DamageReport.Request;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Response;
using ToyShelf.Application.Services;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShipmentAssignmentController : ControllerBase
	{
		private readonly IShipmentAssignmentService _service;

		public ShipmentAssignmentController(IShipmentAssignmentService service)
		{
			_service = service;
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<BaseResponse<ShipmentAssignmentResponse>> Create(
			CreateShipmentAssignmentRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _service.CreateAsync(request, currentUser);

			return BaseResponse<ShipmentAssignmentResponse>
				.Ok(result, "Shipment assignment created");
		}

		// Create from damage report - Gôm đơn thông minh từ báo cáo hư hại
		//[HttpPost("from-damage-report")]
		//[Authorize(Roles = "Admin")] 
		//public async Task<ActionResponse> CreateFromDamageReport(
		//	[FromBody] CreateFromDamageRequest request, 
		//	[FromServices] ICurrentUser currentUser)
		//{
		//	// Gọi hàm xử lý gôm đơn thông minh trong Service
		//	await _service.CreateFromDamageReportAsync(
		//		request.DamageReportId,
		//		request.WarehouseLocationId,
		//		currentUser);

		//	return ActionResponse.Ok("Shipment assignment created or updated from damage report");
		//}

		// ================= ASSIGN SHIPPER =================
		[HttpPatch("assign-shipper")]
		[Authorize(Roles = "Warehouse")]
		public async Task<ActionResponse> AssignShipper(
			AssignShipperRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			await _service.AssignShipperAsync(request, currentUser);

			return ActionResponse.Ok("Shipper assigned successfully");
		}


		[HttpPatch("{id}/accept")]
		[Authorize(Roles = "Warehouse")]
		public async Task<ActionResponse> Accept(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _service.AcceptAsync(id, currentUser);

			return ActionResponse.Ok("Assignment accepted");
		}

		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Warehouse")]
		public async Task<ActionResponse> Reject(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _service.RejectAsync(id, currentUser);

			return ActionResponse.Ok("Assignment rejected");
		}

		[HttpGet("my")]
		[Authorize(Roles = "Warehouse")]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetMy(
				[FromServices] ICurrentUser currentUser,
				[FromQuery] AssignmentType? type,
				[FromQuery] AssignmentStatus? status)
		{
			var result = await _service.GetMyAssignments(currentUser, type, status);

			return BaseResponse<IEnumerable<ShipmentAssignmentResponse>>
				.Ok(result, "Assignments retrieved");
		}

		[HttpGet("store-order/{storeOrderId}")]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetByStoreOrder(Guid storeOrderId)
		{
			var result = await _service.GetByStoreOrderId(storeOrderId);

			return BaseResponse<IEnumerable<ShipmentAssignmentResponse>>
				.Ok(result, "Store order assignments retrieved successfully");
		}

		[HttpGet("shelf-order/{shelfOrderId}")]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetByShelfOrder(Guid shelfOrderId)
		{
			var result = await _service.GetByShelfOrderId(shelfOrderId);

			return BaseResponse<IEnumerable<ShipmentAssignmentResponse>>
				.Ok(result, "Shelf order assignments retrieved successfully");
		}

		[HttpGet("damage-report/{damageReportId}")]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetByDamageReport(Guid damageReportId)
		{
			var result = await _service.GetByDamageReportId(damageReportId);

			return BaseResponse<IEnumerable<ShipmentAssignmentResponse>>
				.Ok(result, "Damage report assignments retrieved successfully");
		}

		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetAll(
			[FromQuery] AssignmentType? type,
			[FromQuery] AssignmentStatus? status)
		{
			var result = await _service.GetAllAsync(type, status);

			return BaseResponse<IEnumerable<ShipmentAssignmentResponse>>
				.Ok(result, "Get all assignments successfully");
		}


		[HttpGet("{id}")]
		public async Task<BaseResponse<ShipmentAssignmentResponse>> GetById(Guid id)
		{
			var result = await _service.GetByIdAsync(id);

			return BaseResponse<ShipmentAssignmentResponse>
				.Ok(result, "Assignment retrieved successfully");
		}
	}
}
