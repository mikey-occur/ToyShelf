using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Partner.Request;
using ToyShelf.Application.Models.Partner.Response;
using ToyShelf.Application.Models.Store.Response;
using ToyShelf.Application.Services;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PartnerController : ControllerBase
	{
		private readonly IPartnerService _partnerService;

		public PartnerController(IPartnerService partnerService)
		{
			_partnerService = partnerService;
		}

		// ================== CREATE ==================
		[HttpPost]
		public async Task<ActionResult<BaseResponse<PartnerResponse>>> Create(
			[FromBody] CreatePartnerRequest request)
		{
			var result = await _partnerService.CreateAsync(request);
			return BaseResponse<PartnerResponse>
				.Ok(result, "Partner created successfully");
		}

		// ================== GET ==================
		[HttpGet]
		public async Task<ActionResult<BaseResponse<IEnumerable<PartnerResponse>>>> GetPartners([FromQuery] bool? isActive)
		{
			var result = await _partnerService.GetPartnersAsync(isActive);
			return BaseResponse<IEnumerable<PartnerResponse>>
				.Ok(result, "Partners retrieved successfully");
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<PartnerResponse>>> GetById(Guid id)
		{
			var result = await _partnerService.GetByIdAsync(id);
			return BaseResponse<PartnerResponse>
				.Ok(result, "Partner retrieved successfully");
		}

		// ================== UPDATE ==================
		[HttpPut("{id}")]
		public async Task<ActionResult<BaseResponse<PartnerResponse>>> Update(
			Guid id,
			[FromBody] UpdatePartnerRequest request)
		{
			var result = await _partnerService.UpdateAsync(id, request);
			return BaseResponse<PartnerResponse>
				.Ok(result, "Partner updated successfully");
		}

		// ================== DISABLE / RESTORE ==================
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _partnerService.DisableAsync(id);
			return ActionResponse.Ok("Partner disabled successfully");
		}

		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _partnerService.RestoreAsync(id);
			return ActionResponse.Ok("Partner restored successfully");
		}

		// ================== DELETE (HARD) ==================
		[HttpDelete("{id}")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _partnerService.DeleteAsync(id);
			return ActionResponse.Ok("Partner deleted successfully");
		}
	}
}
