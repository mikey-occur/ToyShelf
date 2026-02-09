using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Color.Request;
using ToyShelf.Application.Models.Color.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ColorController : ControllerBase
	{
		private readonly IColorService _colorService;
		public ColorController(IColorService colorService)
		{
			_colorService = colorService;
		}

		//==== CREATE =====
		/// <summary>
		/// Create Color.
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<BaseResponse<ColorResponse>>> Create(
			[FromBody] ColorRequest request)
		{
			var result = await _colorService.CreateAsync(request);

			return BaseResponse<ColorResponse>.Ok(result, "Color created successfully");
		}

		// ===== GET ALL COLORS =====
		/// <summary>
		/// Get all colors.
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<ColorResponse>>> GetColors()
		{
			var result = await _colorService.GetColorsAsync();
			return BaseResponse<IEnumerable<ColorResponse>>.Ok(result, "Colors retrieved successfully");
		}

		// ===== GET BY ID =====
		/// <summary>
		/// Get color by id.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<ColorResponse?>> GetById(Guid id)
		{
			var result = await _colorService.GetByIdAsync(id);
			return BaseResponse<ColorResponse?>.Ok(result, "Color retrieved successfully");
		}

		// ===== UPDATE COLOR =====
		/// <summary>
		/// Update Color.
		/// </summary>
		[HttpPut("{id}")]
		public async Task<BaseResponse<ColorResponse>> Update(Guid id, [FromBody] ColorRequest request)
		{
			var result = await _colorService.UpdateAsync(id, request);

			return BaseResponse<ColorResponse>.Ok(result, "Color updated successfully");
		}

		// ===== DELETE COLOR =====
		/// <summary>
		/// Delete Color.
		/// </summary>
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _colorService.DeleteAsync(id);
			return ActionResponse.Ok("Color deleted successfully");
		}
	}
}
