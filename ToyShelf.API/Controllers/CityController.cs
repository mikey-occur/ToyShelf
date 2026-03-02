using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.City.Request;
using ToyShelf.Application.Models.City.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CityController : ControllerBase
	{
		private readonly ICityService _cityService;

		public CityController(ICityService cityService)
		{
			_cityService = cityService;
		}

		// ===== CREATE =====
		/// <summary>
		/// Create City.
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<BaseResponse<CityResponse>>> Create(
			[FromBody] CityRequest request)
		{
			var result = await _cityService.CreateAsync(request);

			return BaseResponse<CityResponse>
				.Ok(result, "City created successfully");
		}

		// ===== GET ALL =====
		/// <summary>
		/// Get all cities.
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<CityResponse>>> GetCities()
		{
			var result = await _cityService.GetAsync();

			return BaseResponse<IEnumerable<CityResponse>>
				.Ok(result, "Cities retrieved successfully");
		}

		// ===== GET BY ID =====
		/// <summary>
		/// Get city by id.
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<CityResponse?>> GetById(Guid id)
		{
			var result = await _cityService.GetByIdAsync(id);

			return BaseResponse<CityResponse?>
				.Ok(result, "City retrieved successfully");
		}

		// ===== UPDATE =====
		/// <summary>
		/// Update City.
		/// </summary>
		[HttpPut("{id}")]
		public async Task<BaseResponse<CityResponse>> Update(
			Guid id,
			[FromBody] CityRequest request)
		{
			var result = await _cityService.UpdateAsync(id, request);

			return BaseResponse<CityResponse>
				.Ok(result, "City updated successfully");
		}

		// ===== DELETE =====
		/// <summary>
		/// Delete City.
		/// </summary>
		[HttpDelete("{id}/delete")]
		public async Task<ActionResult<ActionResponse>> Delete(Guid id)
		{
			await _cityService.DeleteAsync(id);

			return ActionResponse
				.Ok("City deleted successfully");
		}
	}
}
