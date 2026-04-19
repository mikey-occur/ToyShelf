using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.InventoryShelf.Response;
using ToyShelf.Application.Models.Shelf.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InventoryShelfController : ControllerBase
	{
		private readonly IInventoryShelfService _inventoryShelfService;

		public InventoryShelfController(IInventoryShelfService inventoryShelfService)
		{
			_inventoryShelfService = inventoryShelfService;
		}

		/// <summary>
		/// Lấy danh sách kệ theo vị trí kho.
		/// </summary>
		/// <param name="locationId">ID của kho (InventoryLocationId)</param>
		[HttpGet("location/{locationId}")]
		public async Task<BaseResponse<LocationShelvesResponse?>> GetByLocation(Guid locationId)
		{
			var result = await _inventoryShelfService.GetShelvesByLocationAsync(locationId);
			return BaseResponse<LocationShelvesResponse?>
				.Ok(result, "Danh sách kệ theo vị trí đã được tải thành công.");
		}


		/// <summary>
		/// Lấy danh sách kệ phân bổ.
		/// </summary>
		/// <param name="shelfTypeId">ID của loại kệ (ShelfTypeId)</param>
		[HttpGet("distribution/{shelfTypeId}")]
		public async Task<BaseResponse<List<ShelfDistributionResponse>>> GetDistribution(Guid shelfTypeId)
		{
			var result = await _inventoryShelfService.GetShelfDistributionsAsync(shelfTypeId);

			return BaseResponse<List<ShelfDistributionResponse>>
				.Ok(result, "Lấy thông tin phân bổ kệ thành công.");
		}
	}
}
