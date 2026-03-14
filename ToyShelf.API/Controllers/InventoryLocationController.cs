using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.InventoryLocation.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InventoryLocationController : ControllerBase
	{
		private readonly IInventoryLocationService _inventoryLocationService;

		public InventoryLocationController(IInventoryLocationService inventoryLocationService)
		{
			_inventoryLocationService = inventoryLocationService;
		}

		// GET: api/inventorylocation
		[HttpGet]
		public async Task<ActionResult<BaseResponse<IEnumerable<InventoryLocationResponse>>>> GetInventoryLocations([FromQuery] bool? isActive)
		{
			var result = await _inventoryLocationService.GetInventoryLocationsAsync(isActive);

			return BaseResponse<IEnumerable<InventoryLocationResponse>>
				.Ok(result, "Inventory locations retrieved successfully");
		}

		// GET: api/inventorylocation/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<InventoryLocationResponse>>> GetById(Guid id)
		{
			var result = await _inventoryLocationService.GetByIdAsync(id);

			return BaseResponse<InventoryLocationResponse>
				.Ok(result, "Inventory location retrieved successfully");
		}

		// DISABLE
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _inventoryLocationService.DisableAsync(id);

			return ActionResponse.Ok("Inventory location disabled successfully");
		}

		// RESTORE
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _inventoryLocationService.RestoreAsync(id);

			return ActionResponse.Ok("Inventory location restored successfully");
		}
	}
}
