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
		/// Lấy danh sách hàng tồn kho của một kho hàng cụ thể. Xài cho khi tạo Shipment
		/// </summary>
		[HttpGet("warehouse/{warehouseId}/inventory")]
		public async Task<BaseResponse<WarehouseInventoryResponse>> GetWarehouseInventory(Guid warehouseId)
		{
			var result = await _inventoryService.GetWarehouseInventoryAsync(warehouseId);

			return BaseResponse<WarehouseInventoryResponse>
				.Ok(result, "Get warehouse inventory successfully");
		}


		/// <summary>
		/// Admin có thể xem tổng quan về hàng tồn kho của một location (Warehouse hoặc Store),
		/// bao gồm số lượng sản phẩm theo từng loại, màu sắc và tình trạng tồn kho.
		/// Dùng cho báo cáo và quản lý.
		/// </summary>
		[HttpGet("location/{locationId}/inventory-overview")]
		public async Task<BaseResponse<LocationInventoryOverviewResponse>> GetLocationOverview(Guid locationId)
		{
			var result = await _inventoryService.GetLocationInventoryOverviewAsync(locationId);

			return BaseResponse<LocationInventoryOverviewResponse>
				.Ok(result, "Get location inventory overview successfully");
		}


		/// <summary>
		/// Lấy toàn bộ inventory toàn hệ thống (Global Inventory)
		/// </summary>
		/// <returns>Danh sách inventory theo location → product → color + trạng thái</returns>
		[HttpGet("global")]
		public async Task<ActionResult<BaseResponse<IEnumerable<GlobalInventoryResponse>>>> GetGlobalInventory(InventoryLocationType? type)
		{

			var globalInventory = await _inventoryService.GetGlobalInventoryAsync(type);

			return BaseResponse<IEnumerable<GlobalInventoryResponse>>.Ok(
				globalInventory,
				"Get global inventory successfully"
			);
		}
	}
}
