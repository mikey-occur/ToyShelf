using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Inventory.Request;
using ToyShelf.Application.Models.Inventory.Response;
using ToyShelf.Application.Models.InventoryTransaction;
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
		/// Lấy danh sách hàng tồn kho của một kho hàng cụ thể.
		/// </summary>
		[HttpGet("warehouse/{warehouseId}/inventory")]
		public async Task<BaseResponse<WarehouseInventoryResponse>> GetWarehouseInventory(
			Guid warehouseId,
			[FromQuery] int? pageNumber,
			[FromQuery] int? pageSize,
			[FromQuery] bool? isActive,
			[FromQuery] Guid? categoryId,
			[FromQuery] string? searchItem)
		{
			var result = await _inventoryService.GetWarehouseInventoryAsync(
				warehouseId,
				pageNumber,
				pageSize,
				isActive,
				categoryId,
				searchItem);

			return BaseResponse<WarehouseInventoryResponse>
				.Ok(result, "Get warehouse inventory successfully");
		}

		/// <summary>
		/// Admin có thể xem tổng quan về hàng tồn kho của một location (Warehouse hoặc Store),
		/// bao gồm số lượng sản phẩm theo từng loại, màu sắc và tình trạng tồn kho.
		/// Dùng cho báo cáo và quản lý.
		/// </summary>
		[HttpGet("location/{locationId}/inventory-overview")]
		public async Task<BaseResponse<LocationInventoryOverviewResponse>> GetLocationOverview(
				Guid locationId,
				[FromQuery] int? pageNumber,
				[FromQuery] int? pageSize,
				[FromQuery] bool? isActive,
				[FromQuery] Guid? categoryId,
				[FromQuery] string? searchItem)
		{
			var result = await _inventoryService.GetLocationInventoryOverviewAsync(
				locationId,
				pageNumber,
				pageSize,
				isActive,
				categoryId,
				searchItem);

			return BaseResponse<LocationInventoryOverviewResponse>
				.Ok(result, "Get location inventory overview successfully");
		}



		/// <summary>
		/// Lấy toàn bộ inventory toàn hệ thống (Global Inventory)
		/// </summary>
		/// <returns>Danh sách inventory theo location → product → color + trạng thái</returns>
		[HttpGet("global")]
		public async Task<ActionResult<BaseResponse<IEnumerable<GlobalInventoryResponse>>>> GetGlobalInventory(
			[FromQuery] InventoryLocationType? type,
			[FromQuery] int? pageNumber,
			[FromQuery] int? pageSize,
			[FromQuery] bool? isActive,
			[FromQuery] Guid? categoryId,
			[FromQuery] string? searchItem)
		{
			var globalInventory = await _inventoryService.GetGlobalInventoryAsync(
				type, pageNumber, pageSize, isActive, categoryId, searchItem
			);

			return BaseResponse<IEnumerable<GlobalInventoryResponse>>.Ok(
				globalInventory,
				"Get global inventory successfully"
			);
		}


		/// <summary>
		/// Lấy tồn kho chi tiết theo sản phẩm
		/// </summary>
		[HttpGet("product/{productId}/inventory")]
		public async Task<BaseResponse<GlobalProductInventoryByProductResponse>> GetInventoryByProduct(Guid productId)
		{
			var result = await _inventoryService.GetInventoryByProductAsync(productId);

			return BaseResponse<GlobalProductInventoryByProductResponse>.Ok(
				result,
				"Get inventory by product successfully"
			);
		}

		/// <summary>
		/// Lấy tất cả giao dịch (all inventory transactions), có thể filter theo product hoặc location
		/// </summary>
		[HttpGet("transactions")]
		public async Task<BaseResponse<IEnumerable<InventoryTransactionResponse>>> GetAllTransactions(
			[FromQuery] Guid? productId = null,
			[FromQuery] Guid? fromLocationId = null,
			[FromQuery] Guid? toLocationId = null)
		{
			var result = await _inventoryService.GetAllTransactionsAsync(
				productId,
				fromLocationId,
				toLocationId
			);

			return BaseResponse<IEnumerable<InventoryTransactionResponse>>.Ok(
				result,
				"Get all inventory transactions successfully"
			);
		}


		[HttpGet("audit")]
		public async Task<BaseResponse<InventoryAuditResponse>> GetAudit(
			[FromQuery] Guid locationId,
			[FromQuery] Guid productColorId,
			[FromQuery] DateTime? fromDate,
			[FromQuery] DateTime? toDate)
		{
			var result = await _inventoryService.GetInventoryAuditAsync(
				locationId,
				productColorId,
				fromDate,
				toDate
			);

			return BaseResponse<InventoryAuditResponse>
				.Ok(result, "Inventory audit retrieved successfully");
		}
	}
}
