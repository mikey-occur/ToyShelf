using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Application.Models.Dashboard.Response;
namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DashboardController : ControllerBase
	{
		private readonly IOrderService _orderService;

		public DashboardController(IOrderService orderService)
		{
			_orderService = orderService;
		}


		// ===== GET STAT CARD =====
		/// <summary>
		/// Get real-time statistics (Orders, Revenue) for the Dashboard Stat Card.
		/// </summary>
		[HttpGet("stat-card")] 
		public async Task<BaseResponse<StoreDashboardResponse>> GetStatCard(
			[FromQuery] Guid storeId,
			[FromQuery] DateTime? fromDate,
			[FromQuery] DateTime? toDate)
		{
			// Gọi tầng Service xử lý
			var result = await _orderService.GetStoreRevenueAsync(storeId, fromDate, toDate);

			return BaseResponse<StoreDashboardResponse>.Ok(result, "Stat card data retrieved successfully");
		}






















































































		///////////////////////////
	}
}
