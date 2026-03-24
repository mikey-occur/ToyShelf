using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Inventory.Request;
using ToyShelf.Application.Models.Inventory.Response;
using ToyShelf.Domain.Entities;

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
		public async Task<BaseResponse<InventoryResponse>> Refill(
			[FromBody] RefillInventoryRequest request)
		{
			var result = await _inventoryService.RefillAsync(request);

			return BaseResponse<InventoryResponse>
				.Ok(result, "Inventory refilled successfully");
		}

		// ================= GET ALL =================
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<InventoryResponse>>> GetInventories(
		[FromQuery] Guid? locationId,
		[FromQuery] InventoryStatus? status)
		{
			var result = await _inventoryService.GetInventoriesAsync(locationId, status);

			return BaseResponse<IEnumerable<InventoryResponse>>
				.Ok(result, "Inventories retrieved successfully");
		}

		// ================= GET BY ID =================
		[HttpGet("{id}")]
		public async Task<BaseResponse<InventoryResponse>> GetById(Guid id)
		{
			var result = await _inventoryService.GetByIdAsync(id);

			return BaseResponse<InventoryResponse>
				.Ok(result, "Inventory retrieved successfully");
		}

		/// <summary>
		/// Lấy danh sách hàng tồn kho của một kho hàng cụ thể, bao gồm thông tin về sản phẩm, màu sắc và số lượng tồn kho.
		/// </summary>
		[HttpGet("warehouse/{warehouseId}/inventory")]
		public async Task<BaseResponse<WarehouseInventoryResponse>> GetWarehouseInventory(Guid warehouseId)
		{
			var result = await _inventoryService.GetWarehouseInventoryAsync(warehouseId);

			return BaseResponse<WarehouseInventoryResponse>
				.Ok(result, "Get warehouse inventory successfully");
		}


		/// <summary>
		/// Admin có thể xem tổng quan về hàng tồn kho của một kho hàng, bao gồm số lượng sản phẩm theo từng loại, màu sắc và tình trạng tồn kho (còn hàng, sắp hết hàng, hết hàng).
		/// </summary>
		[HttpGet("warehouse/{warehouseId}/inventory-overview")]
		public async Task<BaseResponse<WarehouseInventoryOverviewResponse>> GetOverview(Guid warehouseId)
		{
			var result = await _inventoryService.GetWarehouseInventoryOverviewAsync(warehouseId);

			return BaseResponse<WarehouseInventoryOverviewResponse>
				.Ok(result, "Get warehouse inventory overview successfully");
		}

	}
}
