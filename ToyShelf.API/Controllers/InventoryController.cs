using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Inventory.Request;
using ToyShelf.Application.Models.Inventory.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InventoryController : ControllerBase
	{
		private readonly IInventoryService _inventoryService;

		public InventoryController(IInventoryService inventoryService)
		{
			_inventoryService = inventoryService;
		}

		// ================= REFILL =================
		[HttpPost("refill")]
		public async Task<ActionResult<BaseResponse<InventoryResponse>>> Refill(
			[FromBody] RefillInventoryRequest request)
		{
			var result = await _inventoryService.RefillAsync(request);

			return BaseResponse<InventoryResponse>
				.Ok(result, "Inventory refilled successfully");
		}

		// ================= GET ALL =================
		[HttpGet]
		public async Task<ActionResult<BaseResponse<IEnumerable<InventoryResponse>>>> GetInventories(
		[FromQuery] Guid? locationId,
		[FromQuery] Guid? dispositionId)
		{
			var result = await _inventoryService.GetInventoriesAsync(locationId, dispositionId);

			return BaseResponse<IEnumerable<InventoryResponse>>
				.Ok(result, "Inventories retrieved successfully");
		}


		// ================= GET BY ID =================
		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<InventoryResponse>>> GetById(Guid id)
		{
			var result = await _inventoryService.GetByIdAsync(id);

			return BaseResponse<InventoryResponse>
				.Ok(result, "Inventory retrieved successfully");
		}
	}
}
