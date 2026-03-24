using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Response;
using ToyShelf.Application.Services;

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
		[Authorize(Roles = "Shipper")]
		public async Task<ActionResponse> Accept(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _service.AcceptAsync(id, currentUser);

			return ActionResponse.Ok("Assignment accepted");
		}

		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Shipper")]
		public async Task<ActionResponse> Reject(Guid id, [FromServices] ICurrentUser currentUser)
		{
			await _service.RejectAsync(id, currentUser);

			return ActionResponse.Ok("Assignment rejected");
		}

		[HttpGet("my")]
		[Authorize(Roles = "Shipper")]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetMy(
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _service.GetMyAssignments(currentUser);

			return BaseResponse<IEnumerable<ShipmentAssignmentResponse>>
				.Ok(result, "Assignments retrieved");
		}

		[HttpGet("store-order/{storeOrderId}")]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetByStoreOrder(Guid storeOrderId)
		{
			var result = await _service.GetByStoreOrderId(storeOrderId);

			return BaseResponse<IEnumerable<ShipmentAssignmentResponse>>
				.Ok(result, "Assignments retrieved successfully");
		}

		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ShipmentAssignmentResponse>>> GetAll()
		{
			var result = await _service.GetAllAsync();

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
