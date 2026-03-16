using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.StoreCreationRequest.Request;
using ToyShelf.Application.Models.StoreCreationRequest.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StoreCreationRequestController : ControllerBase
	{
		private readonly IStoreCreationRequestService _service;

		public StoreCreationRequestController(IStoreCreationRequestService service)
		{
			_service = service;
		}

		// ================= CREATE REQUEST =================
		[HttpPost]
		[Authorize(Roles = "PartnerAdmin")]
		public async Task<BaseResponse<StoreCreationRequestResponse>> Create(
			[FromBody] CreateStoreCreationRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _service.CreateAsync(request, currentUser);

			return BaseResponse<StoreCreationRequestResponse>
				.Ok(result, "Store creation request submitted");
		}

		// ================= GET LIST =================
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<StoreCreationRequestResponse>>> GetRequests(
			[FromQuery] StoreRequestStatus? status)
		{
			var result = await _service.GetRequestsAsync(status);

			return BaseResponse<IEnumerable<StoreCreationRequestResponse>>
				.Ok(result, "Get store creation requests successfully");
		}

		// ================= GET BY ID =================
		[HttpGet("{id:guid}")]
		public async Task<BaseResponse<StoreCreationRequestResponse>> GetById(Guid id)
		{
			var result = await _service.GetByIdAsync(id);

			return BaseResponse<StoreCreationRequestResponse>
				.Ok(result, "Get store creation request successfully");
		}

		// ================= REVIEW =================
		[HttpPost("{id:guid}/review")]
		[Authorize(Roles = "Admin")]
		public async Task<BaseResponse<bool>> Review(
			Guid id,
			[FromBody] ReviewStoreCreationRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			await _service.ReviewAsync(id, request, currentUser);

			return BaseResponse<bool>
				.Ok(true, "Store creation request reviewed successfully");
		}

		// ================= DELETE =================
		[HttpDelete("{id:guid}")]
		[Authorize(Roles = "PartnerAdmin")]
		public async Task<BaseResponse<bool>> Delete(Guid id)
		{
			await _service.DeleteAsync(id);

			return BaseResponse<bool>
				.Ok(true, "Store creation request deleted successfully");
		}
	}
}
