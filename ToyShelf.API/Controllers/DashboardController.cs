using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Dashboard.Request;
using ToyShelf.Application.Models.Dashboard.Response;
using ToyShelf.Application.Models.Warehouse.Response;
namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DashboardController : ControllerBase
	{
		private readonly IOrderService _orderService;
		private readonly IDashboardService _dashboardService;
		public DashboardController(IOrderService orderService, IDashboardService dashboardService)
		{
			_orderService = orderService;
			_dashboardService = dashboardService;
		}


		// ===== GET STAT CARD =====
		/// <summary>
		/// Get  (Orders, Revenue) for Stat Card.
		/// </summary>
		[HttpGet("stat-card/store/{storeId:guid}")] 
		public async Task<BaseResponse<StoreDashboardResponse>> GetRevenueStatCard(
			[FromRoute] Guid storeId,
			[FromQuery] DateTime? fromDate,
			[FromQuery] DateTime? toDate)
		{
			// Gọi tầng Service xử lý
			var result = await _dashboardService.GetStoreRevenueAsync(storeId, fromDate, toDate);

			return BaseResponse<StoreDashboardResponse>.Ok(result, "Stat card data retrieved successfully");
		}

		// ===== GET STAT CARD =====
		/// <summary>
		/// Get store revenue for chart. Nếu Truyền week thì chỉ lấy week hiện tại
		/// </summary>

		[HttpGet("store/{storeId:guid}/revenue-chart")]
		public async Task<IActionResult> GetStoreRevenueChart(
		[FromRoute] Guid storeId,
		[FromQuery] StoreChartRequest request) 
		{
			var chartData = await _dashboardService.GetStoreRevenueChartAsync(storeId, request);

			return Ok(new
			{
				success = true,
				data = chartData
			});
		}


		/// <summary>
		/// Get statistics (Revenue, Orders, Commission, Stores) for a Partner.
		/// </summary>
		[HttpGet("partner/{partnerId:guid}/stat-card")]
		public async Task<IActionResult> GetPartnerStatCard(
			[FromRoute] Guid partnerId,
			[FromQuery] DateTime? startDate,
			[FromQuery] DateTime? endDate)
		{
			// 1. Gọi Service để kéo cái data mỏng dính lên
			var result = await _dashboardService.GetPartnerStatCardAsync(partnerId, startDate, endDate);

			// 2. Trả về JSON chuẩn form sếp hay xài
			return Ok(new
			{
				success = true,
				message = "Partner stat card retrieved successfully",
				data = result
			});
		}

		/// <summary>
		/// Get Chart data for Partner Dashboard (Revenue, Orders, Commission grouped by day/month).
		/// </summary>
		[HttpGet("partner/{partnerId:guid}/chart")]
		public async Task<IActionResult> GetPartnerChart(
			[FromRoute] Guid partnerId,
			[FromQuery] PartnerChartRequest request)
		{
			var chartData = await _dashboardService.GetPartnerChartAsync(partnerId, request);

			return Ok(new
			{
				success = true,
				message = "Partner chart data retrieved successfully",
				data = chartData
			});
		}


		/// <summary>
		/// Get statistics (Total Orders, Total Revenue) for the entire system (Admin).
		/// </summary>
		[HttpGet("admin/stat-card")]
		public async Task<IActionResult> GetSystemStatCard(
			[FromQuery] DateTime? fromDate,
			[FromQuery] DateTime? toDate)
		{
			var result = await _dashboardService.GetSystemStatsAsync(fromDate, toDate);

			return Ok(new
			{
				success = true,
				message = "System stat card retrieved successfully",
				data = result
			});
		}

		/// <summary>
		/// Get system revenue for chart (Admin). Nếu truyền week thì lấy theo tuần.
		/// </summary>
		[HttpGet("admin/revenue-chart")]
		public async Task<IActionResult> GetSystemRevenueChart(
			[FromQuery] StoreChartRequest request)
		{
			var chartData = await _dashboardService.GetSystemRevenueChartAsync(request);

			return Ok(new
			{
				success = true,
				message = "System chart data retrieved successfully",
				data = chartData
			});
		}
























































		// ================= WAREHOUSE DASHBOARD =================
		[HttpGet("warehouse/{warehouseId}")]
		public async Task<BaseResponse<WarehouseDashboardResponse>> GetWarehouseDashboard(Guid warehouseId)
		{
			var result = await _dashboardService.GetWarehouseDashboard(warehouseId);

			return BaseResponse<WarehouseDashboardResponse>
				.Ok(result, "Warehouse dashboard retrieved successfully");
		}


















		///////////////////////////
	}
}
