using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceTable.Request;
using ToyShelf.Application.Models.PriceTable.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PriceTableController : ControllerBase
	{
		private readonly IPriceTableService _priceTableService;

		public PriceTableController(IPriceTableService priceTableService)
		{
			_priceTableService = priceTableService;
		}

		/// <summary>
		/// Create Price Table.
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<BaseResponse<PriceTableResponse>>> Create(
			[FromBody] PriceTableRequest request)
		{
			var result = await _priceTableService.CreateAsync(request);

			return BaseResponse<PriceTableResponse>.Ok(result, "Price Table created successfully");
		}

		/// <summary>
		/// Get Price Tables (Filter by isActive).
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<PriceTableResponse>>> GetPriceTables([FromQuery] bool? isActive)
		{
			var result = await _priceTableService.GetPriceTablesAsync(isActive);
			return BaseResponse<IEnumerable<PriceTableResponse>>.Ok(result, "Price Tables retrieved successfully");
		}

		// ===== GET BY ID =====
		/// <summary>
		/// Get Price Table by id.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<PriceTableResponse>> GetById(Guid id)
		{
			var result = await _priceTableService.GetByIdAsync(id);
			return BaseResponse<PriceTableResponse>.Ok(result, "Price Table retrieved successfully");
		}

		// ===== UPDATE =====
		/// <summary>
		/// Update Price Table.
		/// </summary>
		[HttpPut("{id}")]
		public async Task<BaseResponse<PriceTableResponse>> Update(Guid id, [FromBody] PriceTableUpdateRequest request)
		{
			var result = await _priceTableService.UpdateAsync(id, request);

			return BaseResponse<PriceTableResponse>.Ok(result, "Price Table updated successfully");
		}

		// ===== DELETE =====
		/// <summary>
		/// Delete Price Table (Check in use).
		/// </summary>
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _priceTableService.DeleteAsync(id);
			return ActionResponse.Ok("Price Table deleted successfully");
		}

		// ===== DISABLE =====
		/// <summary>
		/// Disable Price Table.
		/// </summary>
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _priceTableService.DisablePriceTableAsync(id);
			return ActionResponse.Ok("Price Table disabled successfully");
		}

		// ===== RESTORE =====
		/// <summary>
		/// Restore Price Table.
		/// </summary>
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _priceTableService.RestorePriceTableAsync(id);
			return ActionResponse.Ok("Price Table restored successfully");
		}
	}
}
