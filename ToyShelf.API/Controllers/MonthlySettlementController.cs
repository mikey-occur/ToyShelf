using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.MonthlySettlement.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MonthlySettlementController : Controller
	{
		
	    private readonly IMonthlySettlementService _settlementService;

		public MonthlySettlementController(IMonthlySettlementService settlementService)
		{
			_settlementService = settlementService;
		}

		// ===== CREATE (GENERATE) =====
		/// <summary>
		/// Action gom bill và chốt sổ cuối tháng
		/// </summary>
		[HttpPost("generate/{year}/{month}")]
		public async Task<BaseResponse<IEnumerable<MonthlySettlementResponse>>> Generate(int year, int month)
		{
			if (month < 1 || month > 12)
				throw new AppException("Invalid month", 400);

			var result = await _settlementService.GenerateMonthlySettlementAsync(month, year);

			return BaseResponse<IEnumerable<MonthlySettlementResponse>>.Ok(result, $"Monthly settlement generated successfully for {month}/{year}");
		}

		/// <summary>
		/// Kế toán xác nhận đã chuyển khoản thành công cho đối tác
		/// </summary>
		[HttpPatch("{id}/pay")]
		public async Task<ActionResult<ActionResponse>> Pay(Guid id)
		{
			// Gọi thẳng hàm PayAsync chỉ với ID
			var isSuccess = await _settlementService.PayAsync(id);

			if (!isSuccess)
				throw new AppException("Monthly settlement not found or already paid", 404);

			return ActionResponse.Ok("Monthly settlement marked as paid successfully");
		}

		/// <summary>
		/// Xem chi tiết 1 phiếu chốt sổ (Kèm danh sách các món đồ chơi bên trong)
		/// </summary>
		[HttpGet("{id}")]
		public async Task<BaseResponse<MonthlySettlementResponse>> GetById(Guid id)
		{
			
			var settlement = await _settlementService.GetByIdAsync(id);

			if (settlement == null)
				throw new AppException("Monthly settlement not found", 404);

			return BaseResponse<MonthlySettlementResponse>.Ok(settlement, "Monthly settlement retrieved successfully");
		}
		/// <summary>
		/// Lấy toàn bộ danh sách phiếu chốt sổ của tất cả đối tác (Cho Admin)
		/// </summary>
		[HttpGet] // <-- THÊM CHỮ "all" VÀO ĐÂY
		public async Task<BaseResponse<IEnumerable<MonthlySettlementResponse>>> GetAll()
		{
			var result = await _settlementService.GetAllAsync();

			return BaseResponse<IEnumerable<MonthlySettlementResponse>>.Ok(
				result,
				"Monthly settlements retrieved successfully");
		}
	}
}
