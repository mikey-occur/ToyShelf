using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PartnerTier.Request;
using ToyShelf.Application.Models.PartnerTier.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PartnerTierController : ControllerBase
	{
		private readonly IPartnerTierService _service;

		public PartnerTierController(IPartnerTierService service)
		{
			_service = service;
		}


		/// <summary>
		/// Create PartnerTier.
		/// </summary>
		// CREATE
		[HttpPost]
		public async Task<ActionResult<BaseResponse<PartnerTierResponse>>> Create([FromBody] PartnerTierRequest request)
		{
			var result = await _service.CreateAsync(request);
			return BaseResponse<PartnerTierResponse>.Ok(result, "Partner Tier created successfully");
		}

		/// <summary>
		/// GetAll PartnerTier.
		/// </summary>
		// GET ALL
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<PartnerTierResponse>>> GetAll()
		{
			var result = await _service.GetAllAsync();
			return BaseResponse<IEnumerable<PartnerTierResponse>>.Ok(result, "Retrieved successfully");
		}

		/// <summary>
		/// Get PartnerTier By Id.
		/// </summary>
		// GET BY ID
		[HttpGet("{id}")]
		public async Task<BaseResponse<PartnerTierResponse>> GetById(Guid id)
		{
			var result = await _service.GetByIdAsync(id);
			return BaseResponse<PartnerTierResponse>.Ok(result, "Retrieved successfully");
		}

		/// <summary>
		/// Update PartnerTier.
		/// </summary>
		// UPDATE
		[HttpPut("{id}")]
		public async Task<BaseResponse<PartnerTierResponse>> Update(Guid id, [FromBody] PartnerTierRequest request)
		{
			var result = await _service.UpdateAsync(id, request);
			return BaseResponse<PartnerTierResponse>.Ok(result, "Partner Tier updated successfully");
		}

		/// <summary>
		/// Delete PartnerTier.
		/// </summary>
		// DELETE
		[HttpDelete("{id}")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _service.DeleteAsync(id);
			return ActionResponse.Ok("Partner Tier deleted successfully");
		}
	}
}
