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


		[HttpPatch("{id}/partner-approve")]
		[Authorize(Roles = "PartnerAdmin")]
		public async Task<ActionResult<ActionResponse>> PartnerApprove(
			Guid id,
			[FromServices] ICurrentUser currentUser)
		{
			await _damageReportService.PartnerApproveAsync(id, currentUser);
			return Ok(ActionResponse.Ok("Đối tác đã xác nhận báo cáo. Đang chờ Admin hệ thống phê duyệt cuối cùng."));
		}

		[HttpPatch("{id}/admin-approve")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> Approve(
			Guid id,
			[FromBody] string? adminNote,
			[FromServices] ICurrentUser currentUser)
		{
			await _damageReportService.ApproveAsync(id, adminNote, currentUser);
			return ActionResponse.Ok("Báo cáo đã duyệt. Hàng hóa đã chuyển sang trạng thái chờ thu hồi.");
		}

		[HttpPost("{id}/create-assignment")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<ActionResponse>> CreateAssignment(
			Guid id,
			[FromQuery] Guid warehouseLocationId,
			[FromServices] ICurrentUser currentUser)
		{
			await _damageReportService.CreateRecallAssignmentAsync(id, warehouseLocationId, currentUser);
			return ActionResponse.Ok("Đã tạo lệnh thu hồi và gửi tới đội vận chuyển.");
		}

		[HttpPatch("{id}/reject")]
		[Authorize(Roles = "Admin,PartnerAdmin")]
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
