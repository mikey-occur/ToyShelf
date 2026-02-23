using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceSegment.Request;
using ToyShelf.Application.Models.PriceSegment.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PriceSegmentController : ControllerBase
	{
		private readonly IPriceSegmentService _priceSegmentService;


		//==== CREATE =====
		/// <summary>
		/// Create Price Segment.
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<BaseResponse<PriceSegmentResponse>>> Create(
			[FromBody] PriceSegmentRequest request)
		{
			var result = await _priceSegmentService.CreateAsync(request);

			return BaseResponse<PriceSegmentResponse>.Ok(result, "Price segment created successfully");
		}

		public PriceSegmentController(IPriceSegmentService priceSegmentService)
		{
			_priceSegmentService = priceSegmentService;
		}

		// ===== GET ALL PRICE SEGMENTS =====
		/// <summary>
		/// Get all price segments.
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<PriceSegmentResponse>>> GetAll()
		{
			var result = await _priceSegmentService.GetAllAsync();
			return BaseResponse<IEnumerable<PriceSegmentResponse>>.Ok(result, "Price segments retrieved successfully");
		}

		// ===== GET BY ID =====
		/// <summary>
		/// Get price segment by id.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<PriceSegmentResponse?>> GetById(Guid id)
		{

			var result = await _priceSegmentService.GetByIdAsync(id);
			return BaseResponse<PriceSegmentResponse?>.Ok(result, "Price segment retrieved successfully");
		}

		/// <summary>
		/// Update Price Segment.
		/// </summary>
		[HttpPut("{id}")]
		public async Task<BaseResponse<PriceSegmentResponse>> Update(Guid id, [FromBody] PriceSegmentUpdateRequest request)
		{
			var result = await _priceSegmentService.UpdateAsync(id, request);

			return BaseResponse<PriceSegmentResponse>.Ok(result, "Price segment updated successfully");
		}

		/// <summary>
		/// Delete Price Segment.
		/// </summary>
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _priceSegmentService.DeleteAsync(id);
			return ActionResponse.Ok("Price segment deleted successfully");
		}
	}
}
