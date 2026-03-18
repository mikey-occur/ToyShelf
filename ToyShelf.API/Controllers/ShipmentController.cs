using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.Shipment.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShipmentController : ControllerBase
	{
		private readonly IShipmentService _shipmentService;

		public ShipmentController(IShipmentService shipmentService)
		{
			_shipmentService = shipmentService;
		}


		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ShipmentResponse>>> GetAll(ShipmentStatus? shipmentStatus)
		{
			var result = await _shipmentService.GetAllAsync(shipmentStatus);

			return BaseResponse<IEnumerable<ShipmentResponse>>
				.Ok(result, "Get shipments successfully");
		}

		[HttpGet("assignment/{assignmentId}")]
		public async Task<BaseResponse<ShipmentResponse>> GetByAssignment(Guid assignmentId)
		{
			var result = await _shipmentService.GetByAssignmentIdAsync(assignmentId);

			return BaseResponse<ShipmentResponse>
				.Ok(result, "Get shipment successfully");
		}


		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<BaseResponse<ShipmentResponse>> Create(
			[FromBody] CreateShipmentRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _shipmentService.CreateAsync(request, currentUser);

			return BaseResponse<ShipmentResponse>
				.Ok(result, "Shipment created successfully");
		}

		[HttpPatch("{id}/pickup")]
		[Authorize(Roles = "Shipper")]
		public async Task<ActionResult<ActionResponse>> Pickup(
			Guid id,
			[FromBody] UploadShipmentMediaRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			await _shipmentService.PickupAsync(id, request, currentUser);

			return ActionResponse.Ok("Shipment picked up successfully");
		}

		[HttpPatch("{id}/delivery")]
		[Authorize(Roles = "Shipper")]
		public async Task<ActionResult<ActionResponse>> Delivery(
			Guid id,
			[FromBody] UploadShipmentMediaRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			await _shipmentService.DeliveryAsync(id, request, currentUser);

			return ActionResponse.Ok("Shipment delivered successfully");
		}

		[HttpPatch("{id}/receive")]
		[Authorize(Roles = "Partner")]
		public async Task<ActionResult<ActionResponse>> Receive(Guid id)
		{
			await _shipmentService.ReceiveAsync(id);

			return ActionResponse.Ok("Shipment received successfully");
		}
	}
}
