using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shelf.Request;
using ToyShelf.Application.Models.Shelf.Response;

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

        // ===== CREATE =====
        /// <summary>
        /// Create a new shelf.
        /// </summary>
        [HttpPost]
        public async Task<BaseResponse<ShelfResponse>> Create([FromBody] CreateShelfRequest request)
        {
            var result = await _shelfService.CreateAsync(request);
            return BaseResponse<ShelfResponse>.Ok(result, "Shelf created successfully");
        }

        // ===== GET ALL =====
        /// <summary>
        /// Get all shelves.
        /// </summary>
        [HttpGet]
        public async Task<BaseResponse<IEnumerable<ShelfResponse>>> GetAll()
        {
            var result = await _shelfService.GetAllAsync();
            return BaseResponse<IEnumerable<ShelfResponse>>.Ok(result, "Shelves retrieved successfully");
        }

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

        // ===== GET PAGINATED =====
        /// <summary>
        /// Get shelves with pagination and optional status filter.
        /// </summary>
        [HttpGet("paginated")]
        public async Task<BaseResponse<PaginatedResult<ShelfResponse>>> GetPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            var result = await _shelfService.GetPaginatedAsync(pageNumber, pageSize, status);
            return BaseResponse<PaginatedResult<ShelfResponse>>.Ok(result, "Shelves retrieved successfully");
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
    }
}
