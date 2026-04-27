using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.CommissionHistory.Response;

namespace ToyShelf.API.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class CommissionHistoryController : Controller
	{
		private readonly ICommissionHistoryService _commissionHistoryService;

		public CommissionHistoryController(ICommissionHistoryService commissionHistoryService)
		{
			_commissionHistoryService = commissionHistoryService;
		}

        [HttpGet("history")]
        public async Task<BaseResponse<PaginatedResult<CommissionHistoryResponse>>> GetHistory(
            [FromQuery] Guid? partnerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            // --- THÊM 3 BIẾN NÀY ĐỂ LỌC ---
            [FromQuery] Guid? storeId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Truyền đủ tham số xuống Service (đảm bảo đúng thứ tự bạn đã khai báo trong Interface)
            var (items, totalCount) = await _commissionHistoryService.GetHistoriesPaginatedAsync(
                pageNumber,
                pageSize,
                partnerId,
                keyword,
                storeId,
                fromDate,
                toDate);

            var result = new PaginatedResult<CommissionHistoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return BaseResponse<PaginatedResult<CommissionHistoryResponse>>.Ok(result, "Lấy lịch sử hoa hồng thành công!");
        }
    }
}
