using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.InventoryDisposition.Request;
using ToyShelf.Application.Models.InventoryDisposition.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class InventoryDispositionController : ControllerBase
	{
		private readonly IInventoryDispositionService _service;

		public InventoryDispositionController(IInventoryDispositionService service)
		{
			_service = service;
		}

		// ================= CREATE =================
		[HttpPost]
		public async Task<ActionResult<BaseResponse<InventoryDispositionResponse>>> Create(
			[FromBody] CreateInventoryDispositionRequest request)
		{
			var result = await _service.CreateAsync(request);

			return BaseResponse<InventoryDispositionResponse>
				.Ok(result, "Inventory disposition created successfully");
		}

		// ================= GET =================
		[HttpGet]
		public async Task<ActionResult<BaseResponse<IEnumerable<InventoryDispositionResponse>>>> GetAll()
		{
			var result = await _service.GetAllAsync();

			return BaseResponse<IEnumerable<InventoryDispositionResponse>>
				.Ok(result, "Inventory dispositions retrieved successfully");
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<BaseResponse<InventoryDispositionResponse>>> GetById(Guid id)
		{
			var result = await _service.GetByIdAsync(id);

			return BaseResponse<InventoryDispositionResponse>
				.Ok(result, "Inventory disposition retrieved successfully");
		}

		// ================= UPDATE =================
		[HttpPut("{id}")]
		public async Task<ActionResult<BaseResponse<InventoryDispositionResponse>>> Update(
			Guid id,
			[FromBody] UpdateInventoryDispositionRequest request)
		{
			var result = await _service.UpdateAsync(id, request);

			return BaseResponse<InventoryDispositionResponse>
				.Ok(result, "Inventory disposition updated successfully");
		}

		// ================= DELETE =================
		[HttpDelete("{id}")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _service.DeleteAsync(id);

			return ActionResponse.Ok("Inventory disposition deleted successfully");
		}
	}
}
