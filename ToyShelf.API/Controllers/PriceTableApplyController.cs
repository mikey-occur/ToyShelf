using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceTableApply.Request;
using ToyShelf.Application.Models.PriceTableApply.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PriceTableApplyController : ControllerBase
	{
		private readonly IPriceTableApplyService _service;

		public PriceTableApplyController(IPriceTableApplyService service)
		{
			_service = service;
		}

		/// <summary>
		/// Create Price Table Apply.
		/// </summary>
		[HttpPost]
		public async Task<BaseResponse<PriceTableApplyResponse>> Create(
			[FromBody] PriceTableApplyRequest request)
		{
			var result = await _service.CreateAsync(request);
			return BaseResponse<PriceTableApplyResponse>
				.Ok(result, "Price table apply created successfully");
		}

		/// <summary>
		/// Get all Price Table Apply.
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<PriceTableApplyResponse>>> GetAll(
			[FromQuery] bool? isActive)
		{
			var result = await _service.GetAllAsync(isActive);
			return BaseResponse<IEnumerable<PriceTableApplyResponse>>
				.Ok(result, "Price table apply retrieved successfully");
		}

		/// <summary>
		/// Delete Price Table Apply (Hard delete).
		/// </summary>
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _service.DeleteAsync(id);
			return ActionResponse.Ok("Price table apply deleted successfully");
		}

		/// <summary>
		/// Disable Price Table Apply (Soft delete).
		/// </summary>
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _service.DisablePriceTableApply(id);
			return ActionResponse.Ok("Price table apply disabled successfully");
		}

		/// <summary>
		/// Restore Price Table Apply.
		/// </summary>
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _service.RestorePriceTableApplyAsync(id);
			return ActionResponse.Ok("Price table apply restored successfully");
		}
	}
}