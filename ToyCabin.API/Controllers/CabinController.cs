using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Cabin.Request;
using ToyCabin.Application.Models.Cabin.Response;

namespace ToyCabin.API.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
    public class CabinController : ControllerBase
    {
       private readonly ICabinService _cabinService;
        public CabinController(ICabinService cabinService)
        {
            _cabinService = cabinService;
        }

        /// <summary>
        /// Gets all cabins.
        /// </summary>
        [HttpGet("all")]
        public async Task<BaseResponse<IEnumerable<CabinResponse>>> GetAllCabins()
        {
            var cabins = await _cabinService.GetAllCabinsAsync();
            return BaseResponse<IEnumerable<CabinResponse>>.Ok(cabins, "Cabins retrieved successfully");
        }

        /// <summary>
        /// Gets cabin by ID.
        /// </summary>
        [HttpGet("{cabinId}")]
        public async Task<BaseResponse<CabinResponse>> GetCabinById(Guid cabinId)
        {
            var cabin = await _cabinService.GetCabinByIdAsync(cabinId);
            return BaseResponse<CabinResponse>.Ok(cabin, "Cabin retrieved successfully");
        }

        /// <summary>
        /// Create a new cabin.
        /// </summary>
        [HttpPost("create")]
        public async Task<BaseResponse<CabinResponse>> CreateCabin([FromBody] CreateCabinRequest request)
        {
            var cabin = await _cabinService.CreateCabinAsync(request);
            return BaseResponse<CabinResponse>.Ok(cabin, "Cabin created successfully");
        }

        /// <summary>
        /// Update an existing cabin.
        /// </summary>
        [HttpPut("{cabinId}")]
        public async Task<ActionResponse> UpdateCabin(Guid cabinId, [FromBody] UpdateCabinRequest request)
        {
            var cabin = await _cabinService.UpdateCabinAsync(cabinId, request);
            return ActionResponse.Ok("Cabin updated successfully");
        }

        /// <summary>
        /// Delete a cabin by ID.
        /// </summary>
        [HttpDelete("{cabinId}")]
        public async Task<ActionResponse> DeleteCabin(Guid cabinId)
        {
            var result = await _cabinService.DeleteCabinAsync(cabinId);
            return ActionResponse.Ok("Cabin deleted successfully");
        }
    }
}