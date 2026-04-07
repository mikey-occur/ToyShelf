using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.DamageReport.Request;
using ToyShelf.Application.Models.DamageReport.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DamageReportController : ControllerBase
	{
		private readonly IDamageReportService _damageReportService;

		public DamageReportController(IDamageReportService damageReportService)
		{
			_damageReportService = damageReportService;
		}

		// ================= CREATE (Partner báo hỏng/bảo hành) =================
		[HttpPost]
		[Authorize(Roles = "Partner")]
		public async Task<BaseResponse<DamageReportResponse>> Create(
			[FromBody] CreateDamageReportRequest request,
			[FromServices] ICurrentUser currentUser)
		{
			var result = await _damageReportService.CreateAsync(request, currentUser);

			return BaseResponse<DamageReportResponse>
				.Ok(result, "Damage report created successfully. Waiting for admin review.");
		}

		// ================= GET ALL (Admin hoặc Partner xem danh sách) =================
		[HttpGet]
		[Authorize]
		public async Task<BaseResponse<IEnumerable<DamageReportResponse>>> GetAll([FromQuery] DamageStatus? status)
		{
			var result = await _damageReportService.GetAllAsync(status);

			return BaseResponse<IEnumerable<DamageReportResponse>>
				.Ok(result, "Damage reports retrieved successfully");
		}

		// ================= GET BY ID =================
		[HttpGet("{id}")]
		[Authorize]
		public async Task<BaseResponse<DamageReportResponse>> GetById(Guid id)
		{
			var result = await _damageReportService.GetByIdAsync(id);

			return BaseResponse<DamageReportResponse>
				.Ok(result, "Damage report retrieved successfully");
		}

		// ================= APPROVE (Admin duyệt & Chỉ định kho thu hồi) =================
		[HttpPatch("{id}/approve")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Approve(
			Guid id,
			[FromBody] ApproveDamageRequest request, 
			[FromServices] ICurrentUser currentUser)
		{
			await _damageReportService.ApproveAsync(id, request.WarehouseLocationId, request.AdminNote, currentUser);

			return ActionResponse.Ok("Báo cáo đã được duyệt. Lệnh thu hồi đã được gửi tới Kho chỉ định.");
		}

		// ================= REJECT (Admin từ chối báo cáo) =================
		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Reject(
			Guid id,
			[FromBody] string? adminNote,
			[FromServices] ICurrentUser currentUser)
		{
			await _damageReportService.RejectAsync(id, adminNote, currentUser);

			return ActionResponse.Ok("Damage report rejected.");
		}
	}
}
