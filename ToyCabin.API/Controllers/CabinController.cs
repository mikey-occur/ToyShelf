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
        /// Gets all active cabins.
        /// </summary>
        [HttpGet("active")]
        public async Task<BaseResponse<IEnumerable<CabinResponse>>> GetActiveCabins()
        {
            var cabins = await _cabinService.GetActiveCabinsAsync();
            return BaseResponse<IEnumerable<CabinResponse>>.Ok(cabins, "Active cabins retrieved successfully");
        }

        /// <summary>
        /// Gets all inactive cabins.
        /// </summary>
        [HttpGet("inactive")]
        public async Task<BaseResponse<IEnumerable<CabinResponse>>> GetInactiveCabins()
        {
            var cabins = await _cabinService.GetInactiveCabinsAsync();
            return BaseResponse<IEnumerable<CabinResponse>>.Ok(cabins, "Inactive cabins retrieved successfully");
        }

        /// <summary>
        /// Gets all online cabins.
        /// </summary>
        [HttpGet("online")]
        public async Task<BaseResponse<IEnumerable<CabinResponse>>> GetOnlineCabins()
        {
            var cabins = await _cabinService.GetAllOnlineCabinsAsync();
            return BaseResponse<IEnumerable<CabinResponse>>.Ok(cabins, "Online cabins retrieved successfully");
        }

        /// <summary>
        /// Gets all offline cabins.
        /// </summary>
        [HttpGet("offline")]
        public async Task<BaseResponse<IEnumerable<CabinResponse>>> GetOfflineCabins()
        {
            var cabins = await _cabinService.GetAllOfflineCabinsAsync();
            return BaseResponse<IEnumerable<CabinResponse>>.Ok(cabins, "Offline cabins retrieved successfully");
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
        public async Task<BaseResponse<CabinResponse>> UpdateCabin(Guid cabinId, [FromBody] UpdateCabinRequest request)
        {
            var cabin = await _cabinService.UpdateCabinAsync(cabinId, request);
            return BaseResponse<CabinResponse>.Ok(cabin, "Cabin updated successfully");
        }

        /// <summary>
        /// Delete a cabin by ID.
        /// </summary>
        [HttpDelete("{cabinId}")]
        public async Task<BaseResponse<bool>> DeleteCabin(Guid cabinId)
        {
            var result = await _cabinService.DeleteCabinAsync(cabinId);
            return BaseResponse<bool>.Ok(result, "Cabin deleted successfully");
        }
    }
}