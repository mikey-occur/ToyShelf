using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Store.Request;
using ToyShelf.Application.Models.Store.Response;

namespace ToyShelf.API.Controllers
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
		public async Task<BaseResponse<StoreResponse>> Create(
			[FromBody] CreateStoreRequest request)
		{
			var result = await _storeService.CreateAsync(request);
			return BaseResponse<StoreResponse>
				.Ok(result, "Store created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<StoreResponse>>> GetStores(
				[FromQuery] bool? isActive,
				[FromQuery] Guid? ownerId,
				[FromQuery] string? keyword,
				[FromQuery] Guid? cityId)
		{
			var result = await _storeService.GetStoresAsync(isActive, ownerId, keyword, cityId);

			return BaseResponse<IEnumerable<StoreResponse>>.Ok(
				result,
				"Get stores successfully"
			);
		}


		[HttpGet("{id}")]
		public async Task<BaseResponse<StoreResponse>> GetById(Guid id)
		{
			var result = await _storeService.GetByIdAsync(id);
			return BaseResponse<StoreResponse>
				.Ok(result, "Store retrieved successfully");
		}

		// ================= UPDATE =================
		[HttpPut("{id}")]
		public async Task<BaseResponse<StoreResponse>> Update(
			Guid id,
			[FromBody] UpdateStoreRequest request)
		{
			var result = await _storeService.UpdateAsync(id, request);
			return BaseResponse<StoreResponse>
				.Ok(result, "Store updated successfully");
		}

		// ================= DISABLE / RESTORE =================
		[HttpPatch("{id}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _storeService.DisableAsync(id);
			return ActionResponse.Ok("Store disabled successfully");
		}

		[HttpPatch("{id}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _storeService.RestoreAsync(id);
			return ActionResponse.Ok("Store restored successfully");
		}

		// ================= DELETE =================
		[HttpDelete("{id}")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _storeService.DeleteAsync(id);
			return ActionResponse.Ok("Store deleted successfully");
		}
	}
}
