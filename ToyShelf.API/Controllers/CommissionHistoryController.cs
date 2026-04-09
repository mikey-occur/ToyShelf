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
		[FromQuery] string? keyword = null) // Thêm cục keyword để xài search
			{
				if (pageNumber < 1) pageNumber = 1;
				if (pageSize < 1) pageSize = 10;

				var (items, totalCount) = await _commissionHistoryService.GetHistoriesPaginatedAsync(pageNumber, pageSize, partnerId, keyword);

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
