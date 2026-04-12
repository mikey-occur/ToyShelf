using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shelf.Request;
using ToyShelf.Application.Models.Shelf.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
        private readonly IShelfService _shelfService;

        public ShelfController(IShelfService shelfService)
        {
            _shelfService = shelfService;
        }

		// ===== GET PAGINATED =====
		/// <summary>
		/// Get shelves with pagination and filters (status, id location).
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<PaginatedResult<ShelfResponse>>> GetPaginated(
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] string? status = null,
			[FromQuery] Guid? inventoryLocationId = null)
		{
			var result = await _shelfService.GetPaginatedAsync(
				pageNumber,
				pageSize,
				status,
				inventoryLocationId);

			return BaseResponse<PaginatedResult<ShelfResponse>>
				.Ok(result, "Shelves retrieved successfully");
		}

		// ===== CREATE =====
		/// <summary>
		/// Create a new shelf.
		/// </summary>
		[HttpPost]
		public async Task<BaseResponse<List<ShelfResponse>>> Create([FromBody] CreateShelfRequest request)
		{
			
			var result = await _shelfService.CreateAsync(request);
			return BaseResponse<List<ShelfResponse>>.Ok(result, "Shelves created successfully");
		}


		//[HttpGet]
		//public async Task<BaseResponse<IEnumerable<ShelfResponse>>> GetAll()
		//{
		//    var result = await _shelfService.GetAllAsync();
		//    return BaseResponse<IEnumerable<ShelfResponse>>.Ok(result, "Shelves retrieved successfully");
		//}

		// ===== GET BY ID =====
		/// <summary>
		/// Get shelf by ID.
		/// </summary>
		[HttpGet("{id:guid}")]
        public async Task<BaseResponse<ShelfResponse>> GetById(Guid id)
        {
            var result = await _shelfService.GetByIdAsync(id);
            return BaseResponse<ShelfResponse>.Ok(result, "Shelf retrieved successfully");
        }

        // ===== UPDATE =====
        /// <summary>
        /// Update a shelf.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<BaseResponse<ShelfResponse>> Update(Guid id, [FromBody] UpdateShelfRequest request)
        {
            var result = await _shelfService.UpdateAsync(id, request);
            return BaseResponse<ShelfResponse>.Ok(result, "Shelf updated successfully");
        }

        // ===== DELETE =====
        /// <summary>
        /// Delete a shelf.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResponse> Delete(Guid id)
        {
            await _shelfService.DeleteAsync(id);
            return ActionResponse.Ok("Shelf deleted successfully");
        }

        // ===== DELETE =====
        /// <summary>
        /// Update shelf status
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        public async Task<BaseResponse<ShelfResponse>> UpdateStatus(Guid id, [FromQuery] ShelfStatus status)
        {
            var result = await _shelfService.UpdateShelftStatus(id, status);
            return BaseResponse<ShelfResponse>.Ok(result, "Shelf status updated successfully");

        }
    }
}
