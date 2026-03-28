using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.ShelfType.Request;
using ToyShelf.Application.Models.ShelfType.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShefTypeController : ControllerBase
	{
		private readonly IShelfTypeService _shelfTypeService;

		public ShefTypeController(IShelfTypeService shelfTypeService)
		{
			_shelfTypeService = shelfTypeService;
		}

		/// <summary>
		/// Create ShelfType.
		/// </summary>
		// ===== CREATE SHELFTYPE =====
		[HttpPost]
		public async Task<BaseResponse<ShelfTypeResponse>> Create([FromBody] ShelfTypeRequest request)
		{
			var result = await _shelfTypeService.CreateAsync(request);
			return BaseResponse<ShelfTypeResponse>.Ok(result, "ShelfType created successfully");
		}

		// ===== GET SHELFTYPES (Lọc nâng cao) =====
		/// <summary>
		/// Lấy danh sách mẫu kệ. Hỗ trợ lọc theo trạng thái, tên kệ, và loại sản phẩm (Lego, Toy...).
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ShelfTypeResponse>>> GetShelfTypes(
			[FromQuery] bool? isActive,
			[FromQuery] string? searchName,
			[FromQuery] string? categoryType)
		{
			var result = await _shelfTypeService.GetShelfTypesAsync(isActive, searchName, categoryType);
			return BaseResponse<IEnumerable<ShelfTypeResponse>>.Ok(result, "ShelfTypes retrieved successfully");
		}
		/// <summary>
		/// Get by id
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<ShelfTypeResponse?>> GetById(Guid id)
		{
			var result = await _shelfTypeService.GetByIdAsync(id);
			return BaseResponse<ShelfTypeResponse?>.Ok(result, "ShelfType retrieved successfully");
		}

		/// <summary>
		/// Update ShelfType.
		/// </summary>
		// ===== UPDATE SHELFTYPE =====
		[HttpPut("{id}")]
		public async Task<BaseResponse<ShelfTypeResponse?>> Update(Guid id, [FromBody] ShelfTypeRequest request)
		{
			var result = await _shelfTypeService.UpdateAsync(id, request);
			return BaseResponse<ShelfTypeResponse?>.Ok(result, "ShelfType updated successfully");
		}

		/// <summary>
		/// Delete ShelfType (Hard Delete).
		/// </summary>
		// ===== DELETE SHELFTYPE =====
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _shelfTypeService.DeleteAsync(id);
			return ActionResponse.Ok("ShelfType delete successfully");
		}

		/// <summary>
		/// Disable ShelfType (Soft Delete).
		/// </summary>
		// ===== DISABLE SHELFTYPE =====
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _shelfTypeService.DisableAsync(id);
			return ActionResponse.Ok("ShelfType disabled successfully");
		}

		/// <summary>
		/// Restore ShelfType.
		/// </summary>
		// ===== RESTORE SHELFTYPE =====
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			// (Sếp nhớ thêm hàm RestoreAsync vào Service nhé - logic y chang Disable nhưng gán IsActive = true)
			await _shelfTypeService.RestoreAsync(id);
			return ActionResponse.Ok("ShelfType restore successfully");
		}

	}

}
