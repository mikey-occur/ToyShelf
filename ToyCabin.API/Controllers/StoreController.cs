using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Store.Request;
using ToyCabin.Application.Models.Store.Response;

namespace ToyCabin.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StoreController : ControllerBase
	{
		private readonly IStoreService _storeService;

		public StoreController(IStoreService storeService)
		{
			_storeService = storeService;
		}

		// ================= CREATE =================
		[HttpPost]
		public async Task<ActionResult<BaseResponse<StoreResponse>>> Create(
			[FromBody] CreateStoreRequest request)
		{
			var result = await _storeService.CreateAsync(request);
			return BaseResponse<StoreResponse>
				.Ok(result, "Store created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<ActionResult<BaseResponse<IEnumerable<StoreResponse>>>> GetAll()
		{
			var result = await _storeService.GetAllAsync();
			return BaseResponse<IEnumerable<StoreResponse>>
				.Ok(result, "Stores retrieved successfully");
		}

		[HttpGet("active")]
		public async Task<ActionResult<BaseResponse<IEnumerable<StoreResponse>>>> GetActive()
		{
			var result = await _storeService.GetActiveAsync();
			return BaseResponse<IEnumerable<StoreResponse>>
				.Ok(result, "Active stores retrieved successfully");
		}

		[HttpGet("inactive")]
		public async Task<ActionResult<BaseResponse<IEnumerable<StoreResponse>>>> GetInactive()
		{
			var result = await _storeService.GetInactiveAsync();
			return BaseResponse<IEnumerable<StoreResponse>>
				.Ok(result, "Inactive stores retrieved successfully");
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<StoreResponse>>> GetById(Guid id)
		{
			var result = await _storeService.GetByIdAsync(id);
			return BaseResponse<StoreResponse>
				.Ok(result, "Store retrieved successfully");
		}

		// ================= UPDATE =================
		[HttpPut("{id}")]
		public async Task<ActionResult<BaseResponse<StoreResponse>>> Update(
			Guid id,
			[FromBody] UpdateStoreRequest request)
		{
			var result = await _storeService.UpdateAsync(id, request);
			return BaseResponse<StoreResponse>
				.Ok(result, "Store updated successfully");
		}

		// ================= DISABLE / RESTORE =================
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<BaseResponse<bool>>> Disable(Guid id)
		{
			var result = await _storeService.DisableAsync(id);
			return BaseResponse<bool>
				.Ok(result, "Store disabled successfully");
		}

		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<BaseResponse<bool>>> Restore(Guid id)
		{
			var result = await _storeService.RestoreAsync(id);
			return BaseResponse<bool>
				.Ok(result, "Store restored successfully");
		}

		// ================= DELETE =================
		[HttpDelete("{id}")]
		public async Task<ActionResult<BaseResponse<bool>>> Delete(Guid id)
		{
			var result = await _storeService.DeleteAsync(id);
			return BaseResponse<bool>
				.Ok(result, "Store deleted successfully");
		}
	}
}
