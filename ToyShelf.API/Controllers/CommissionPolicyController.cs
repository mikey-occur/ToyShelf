using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.CommissionPolicy.Request;
using ToyShelf.Application.Models.CommissionPolicy.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CommissionPolicyController : ControllerBase
	{
		private readonly ICommissionPolicyService _service;

		public CommissionPolicyController(ICommissionPolicyService service)
		{
			_service = service;
		}

		//==== CREATE =====
		/// <summary>
		/// Create CommissionPolicy.
		/// </summary>
		// CREATE
		[HttpPost]
		public async Task<ActionResult<BaseResponse<CommissionPolicyResponse>>> Create(
			[FromBody] CommissionPolicyRequest request)
		{
			var result = await _service.CreateAsync(request);
			return BaseResponse<CommissionPolicyResponse>.Ok(result, "Commission Policy created successfully");
		}

		//==== GET ALL =====
		/// <summary>
		/// Get all CommissionPolicy.
		/// </summary>
		// GET ALL
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<CommissionPolicyResponse>>> GetAll()
		{
			var result = await _service.GetAllAsync();
			return BaseResponse<IEnumerable<CommissionPolicyResponse>>.Ok(result, "Retrieved successfully");
		}

		/// <summary>
		/// GET BY TIER (Xem Hạng Vàng được những quyền lợi gì)
		/// </summary>
		// GET ALL
		// GET BY TIER (Xem Hạng Vàng được những quyền lợi gì)
		[HttpGet("tier/{tierId}")]
		public async Task<BaseResponse<IEnumerable<CommissionPolicyResponse>>> GetByTier(Guid tierId)
		{
			var result = await _service.GetByTierIdAsync(tierId);
			return BaseResponse<IEnumerable<CommissionPolicyResponse>>.Ok(result, "Retrieved by Tier successfully");
		}

		/// <summary>
		/// Update
		/// </summary>
		// UPDATE
		[HttpPut("{id}")]
		public async Task<BaseResponse<CommissionPolicyResponse>> Update(Guid id, [FromBody] UpdateCommissionPolicyRequest request)
		{
			var result = await _service.UpdateAsync(id, request);
			return BaseResponse<CommissionPolicyResponse>.Ok(result, "Updated successfully");
		}

		/// <summary>
		/// Delete
		/// </summary>
		// DELETE
		[HttpDelete("{id}")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _service.DeleteAsync(id);
			return ActionResponse.Ok("Deleted successfully");
		}
	}
}
