using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.MonthlySettlement.Request;
using ToyShelf.Application.Models.MonthlySettlement.Response;
using ToyShelf.Application.Services;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MonthlySettlementController : ControllerBase
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
		[HttpGet] 
		public async Task<BaseResponse<IEnumerable<MonthlySettlementResponse>>> GetAll([FromQuery] SettlementFilterRequest request)
		{
			var result = await _settlementService.GetAllFilterAsync(request);

			return BaseResponse<IEnumerable<MonthlySettlementResponse>>.Ok(
				result,
				"Monthly settlements retrieved successfully");
		}


		/// <summary>
		/// Kích hoạt tay tự động chốt sổ cho THÁNG TRƯỚC và trả về toàn bộ danh sách
		/// </summary>
		[HttpPost("calculate-last-month")]
		public async Task<BaseResponse<IEnumerable<MonthlySettlementResponse>>> CalculateLastMonthAndGetAll()
		{
			// 1. Tự động tìm ra tháng trước
			// Ví dụ hôm nay là tháng 3/2026 -> lùi 1 tháng sẽ ra targetDate là tháng 2/2026
			var targetDate = DateTime.UtcNow.AddMonths(-1);
			int targetMonth = targetDate.Month;
			int targetYear = targetDate.Year;

			// 2. Gọi Service để thực hiện chốt sổ cho cái tháng vừa tìm được
			await _settlementService.GenerateMonthlySettlementAsync(targetMonth, targetYear);

			// 3. Lấy lại toàn bộ danh sách mới nhất
			var allSettlements = await _settlementService.GetAllFilterAsync(new SettlementFilterRequest());

			// 4. Trả về kết quả kèm câu thông báo xịn xò
			return BaseResponse<IEnumerable<MonthlySettlementResponse>>.Ok(
				allSettlements,
				$"Đã chốt sổ tự động thành công cho tháng {targetMonth}/{targetYear}!"
			);
		}

		/// <summary>
		/// Cập nhật số tiền hao trừ (Phạt, cấn trừ, phí vận chuyển...).
		/// </summary>
		[HttpPut("{id}/deduction")]
		public async Task<BaseResponse<MonthlySettlementResponse>> UpdateDeduction(Guid id, [FromBody] UpdateDeductionRequest request)
		{
			var result = await _settlementService.UpdateDeductionAsync(id, request.DeductionAmount, request.Note ?? "");

			return BaseResponse<MonthlySettlementResponse>.Ok(result, "Cập nhật số tiền hao trừ và tính lại tổng thành công");
		}

		/// <summary>
		/// Xuất file Excel chốt sổ (Bắt buộc phải chọn Tháng và Năm)
		/// GET: api/MonthlySettlement/export-excel?Month=3&Year=2026
		/// </summary>
		[HttpGet("export-excel")]
		public async Task<IActionResult> ExportExcel([FromQuery] SettlementFilterRequest filter)
		{
			try
			{
			
				var fileBytes = await _settlementService.ExportSettlementsToExcelAsync(filter);

				// Đặt tên file động theo tháng/năm khách chọn (Lúc này chắc chắn Month và Year đã có giá trị)
				string fileName = $"ChotSo_Thang{filter.Month}_Nam{filter.Year}_{DateTime.Now:ddMMyyyy_HHmm}.xlsx";

				//  Khai báo định dạng chuẩn của file Excel để trình duyệt hiểu
				string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				return File(fileBytes, contentType, fileName);
			}
			catch (AppException ex) 
			{
				
				return StatusCode(ex.StatusCode, new { message = ex.Message });
			}
			catch (Exception ex)
			{
				
				return StatusCode(500, new { message = $"Error when export file: {ex.Message}" });
			}
		}

	}
}
