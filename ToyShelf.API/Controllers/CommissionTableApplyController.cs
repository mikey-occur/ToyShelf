using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceTableApply.Request;
using ToyShelf.Application.Models.PriceTableApply.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CommissionTableApplyController : ControllerBase
	{
		private readonly ICommissionTableApplyService _service;

		public CommissionTableApplyController(ICommissionTableApplyService service)
		{
			_service = service;
		}

		/// <summary>
		/// Create CommissionTableApply.
		/// </summary>
		[HttpPost]
		public async Task<BaseResponse<CommissionTableApplyResponse>> Create(
			[FromBody] CommissionTableApply request)
		{
			var result = await _service.CreateAsync(request);
			return BaseResponse<CommissionTableApplyResponse>
				.Ok(result, "Price table apply created successfully");
		}

		/// <summary>
		/// Get all CommissionTableApply.
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<CommissionTableApplyResponse>>> GetAll(
			[FromQuery] bool? isActive)
		{
			var result = await _service.GetAllAsync(isActive);
			return BaseResponse<IEnumerable<CommissionTableApplyResponse>>
				.Ok(result, "Price table apply retrieved successfully");
		}

		/// <summary>
		/// Delete CommissionTableApply (Hard delete).
		/// </summary>
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _service.DeleteAsync(id);
			return ActionResponse.Ok("Price table apply deleted successfully");
		}

		/// <summary>
		/// Disable CommissionTableApply (Soft delete).
		/// </summary>
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _service.DisablePriceTableApply(id);
			return ActionResponse.Ok("Price table apply disabled successfully");
		}

		/// <summary>
		/// Restore CommissionTableApply.
		/// </summary>
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _service.RestorePriceTableApplyAsync(id);
			return ActionResponse.Ok("Price table apply restored successfully");
		}
	}
}