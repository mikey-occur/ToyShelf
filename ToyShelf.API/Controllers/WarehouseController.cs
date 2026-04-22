using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.UserWarehouse.Response;
using ToyShelf.Application.Models.Warehouse.Request;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class WarehouseController : ControllerBase
	{
		private readonly IWarehouseService _warehouseService;

		public WarehouseController(IWarehouseService warehouseService)
		{
			_warehouseService = warehouseService;
		}

		// ================= CREATE =================
		[HttpPost]
		public async Task<BaseResponse<WarehouseResponse>> Create(
			[FromBody] CreateWarehouseRequest request)
		{
			var result = await _warehouseService.CreateAsync(request);
			return BaseResponse<WarehouseResponse>
				.Ok(result, "Warehouse created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<WarehouseResponse>>> GetWarehouses(
			[FromQuery] bool? isActive,
			[FromQuery] Guid? cityId)
		{
			var result = await _warehouseService.GetWarehousesAsync(isActive, cityId);

			return BaseResponse<IEnumerable<WarehouseResponse>>
				.Ok(result, "Warehouses retrieved successfully");
		}

		[HttpGet("{id}")]
		public async Task<BaseResponse<WarehouseResponse>> GetById(Guid id)
		{
			var result = await _warehouseService.GetByIdAsync(id);
			return BaseResponse<WarehouseResponse>
				.Ok(result, "Warehouse retrieved successfully");
		}

		[HttpGet("{id}/detail")]
		public async Task<BaseResponse<WarehouseDetailResponse>> GetDetail(
			Guid id,
			[FromQuery] WarehouseRole? role)
		{
			var result = await _warehouseService
				.GetWarehouseDetailAsync(id, role);

			return BaseResponse<WarehouseDetailResponse>
				.Ok(result, "Warehouse detail retrieved successfully");
		}

		// ================= UPDATE =================
		[HttpPut("{id}")]
		public async Task<BaseResponse<WarehouseResponse>> Update(
			Guid id,
			[FromBody] UpdateWarehouseRequest request)
		{
			var result = await _warehouseService.UpdateAsync(id, request);
			return BaseResponse<WarehouseResponse>
				.Ok(result, "Warehouse updated successfully");
		}

		// ================= DISABLE / RESTORE =================
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _warehouseService.DisableAsync(id);
			return ActionResponse.Ok("Warehouse disabled successfully");
		}

		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _warehouseService.RestoreAsync(id);
			return ActionResponse.Ok("Warehouse restored successfully");
		}

		// ================= DELETE =================
		[HttpDelete("{id}")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _warehouseService.DeleteAsync(id);
			return ActionResponse.Ok("Warehouse deleted successfully");
		}
	}
}
