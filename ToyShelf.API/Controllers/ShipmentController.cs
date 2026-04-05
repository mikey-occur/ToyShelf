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

		[HttpGet("{id}")]
		public async Task<BaseResponse<ShipmentResponse>> GetById(Guid id)
		{
			var result = await _shipmentService.GetByIdAsync(id);

			return BaseResponse<ShipmentResponse>.Ok(result, "Get shipment success");
		}

		[HttpGet("assignment/{assignmentId}")]
		public async Task<BaseResponse<IEnumerable<ShipmentResponse>>> GetByAssignment(Guid assignmentId)
		{
			var result = await _shipmentService.GetByAssignmentIdAsync(assignmentId);

			return BaseResponse<IEnumerable<ShipmentResponse>>
				.Ok(result, "Get shipments successfully");
		}

		/// <summary>
		/// Lấy danh sách shipment theo StoreOrder
		/// </summary>
		[HttpGet("store-order/{storeOrderId}")]
		public async Task<BaseResponse<IEnumerable<ShipmentResponse>>> GetByStoreOrderId(Guid storeOrderId)
		{
			var result = await _shipmentService.GetByStoreOrderIdAsync(storeOrderId);

			return BaseResponse<IEnumerable<ShipmentResponse>>.Ok(
				result,
				"Get shipments by store order successfully"
			);
		}

		[HttpPost]
		[Authorize(Roles = "Warehouse")]
		public async Task<BaseResponse<ShipmentResponse>> Create(
			[FromBody] CreateShipmentRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _shipmentService.CreateAsync(request, currentUser);

			return BaseResponse<ShipmentResponse>
				.Ok(result, "Shipment created successfully");
		}

		[HttpPatch("{id}/pickup")]
		[Authorize(Roles = "Warehouse")]
		public async Task<ActionResult<ActionResponse>> Pickup(
			Guid id,
			[FromBody] UploadShipmentMediaRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			await _shipmentService.PickupAsync(id, request, currentUser);

			return ActionResponse.Ok("Shipment picked up successfully");
		}

		[HttpPatch("{id}/delivery")]
		[Authorize(Roles = "Warehouse")]
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
		public async Task<ActionResult<ActionResponse>> Receive(Guid id, ReceiveShipmentRequest request)
		{
			await _shipmentService.ReceiveAsync(id, request);

			return ActionResponse.Ok("Shipment received successfully");
		}

		//[HttpGet("{shipmentId}/shelves")]
		//[Authorize(Roles = "Partner,Warehouse")]
		//public async Task<BaseResponse<IEnumerable<ShelfSimpleResponse>>> GetShelvesByShipment(Guid shipmentId)
		//{
		//	var result = await _shipmentService.GetShelvesByShipmentAsync(shipmentId);

		//	return BaseResponse<IEnumerable<ShelfSimpleResponse>>
		//		.Ok(result, "Get shelves by shipment successfully");
		//}

		[HttpGet("{shipmentId}/shelf-items")]
		public async Task<BaseResponse<IEnumerable<ShelfShipmentItemResponse>>> GetShelfItems(Guid shipmentId)
		{
			var result = await _shipmentService.GetShelfItemsAsync(shipmentId);

			return BaseResponse<IEnumerable<ShelfShipmentItemResponse>>
				.Ok(result, "Get shelf items successfully");
		}
	}
}
