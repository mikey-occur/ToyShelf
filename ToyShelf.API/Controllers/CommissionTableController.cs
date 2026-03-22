using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceTable.Request;
using ToyShelf.Application.Models.PriceTable.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CommissionTableController : ControllerBase
	{
		private readonly ICommissionTableService _priceTableService;

		public CommissionTableController(ICommissionTableService priceTableService)
		{
			_priceTableService = priceTableService;
		}

		/// <summary>
		/// Create Price Table.
		/// </summary>
		[HttpPost]
		public async Task<BaseResponse<CommissionTableResponse>> Create(
			[FromBody] CommissionTableRequest request)
		{
			var result = await _priceTableService.CreateAsync(request);

			return BaseResponse<CommissionTableResponse>.Ok(result, "Price Table created successfully");
		}

		/// <summary>
		/// Get CommissionTable  (Filter by isActive).
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<CommissionTableResponse>>> GetPriceTables([FromQuery] bool? isActive)
		{
			var result = await _priceTableService.GetPriceTablesAsync(isActive);
			return BaseResponse<IEnumerable<CommissionTableResponse>>.Ok(result, "Price Tables retrieved successfully");
		}

		// ===== GET BY ID =====
		/// <summary>
		/// Get CommissionTable  by id.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<CommissionTableResponse>> GetById(Guid id)
		{
			var result = await _priceTableService.GetByIdAsync(id);
			return BaseResponse<CommissionTableResponse>.Ok(result, "Price Table retrieved successfully");
		}

		// ===== UPDATE =====
		/// <summary>
		/// Update CommissionTable.
		/// </summary>
		[HttpPut("{id}")]
		public async Task<BaseResponse<CommissionTableResponse>> Update(Guid id, [FromBody] CommissionTableUpdateRequest request)
		{
			var result = await _priceTableService.UpdateAsync(id, request);

			return BaseResponse<CommissionTableResponse>.Ok(result, "Price Table updated successfully");
		}

		// ===== DELETE =====
		/// <summary>
		/// Delete CommissionTable (Check in use).
		/// </summary>
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _priceTableService.DeleteAsync(id);
			return ActionResponse.Ok("Price Table deleted successfully");
		}

		// ===== DISABLE =====
		/// <summary>
		/// Disable CommissionTable.
		/// </summary>
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _priceTableService.DisablePriceTableAsync(id);
			return ActionResponse.Ok("Price Table disabled successfully");
		}

		// ===== RESTORE =====
		/// <summary>
		/// Restore CommissionTable.
		/// </summary>
		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _priceTableService.RestorePriceTableAsync(id);
			return ActionResponse.Ok("Price Table restored successfully");
		}
	}
}
