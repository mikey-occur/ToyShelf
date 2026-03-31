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
		[HttpGet("stat-card")] 
		public async Task<BaseResponse<StoreDashboardResponse>> GetRevenueStatCard(
			[FromQuery] Guid storeId,
			[FromQuery] DateTime? fromDate,
			[FromQuery] DateTime? toDate)
		{
			// Gọi tầng Service xử lý
			var result = await _dashboardService.GetStoreRevenueAsync(storeId, fromDate, toDate);

			return BaseResponse<StoreDashboardResponse>.Ok(result, "Stat card data retrieved successfully");
		}


		// ===== GET STAT CARD =====
		/// <summary>
		/// Get  (Orders, Revenue) for Dashboard Card.
		/// </summary>
		[HttpGet("Dash-board")]
		public async Task<BaseResponse<StoreDashboardResponse>> GetRevenueDashBoard(
			[FromQuery] Guid storeId,
			[FromQuery] DateTime? fromDate,
			[FromQuery] DateTime? toDate)
		{
			
			var result = await _dashboardService.GetStoreRevenueAsync(storeId, fromDate, toDate);

			return BaseResponse<StoreDashboardResponse>.Ok(result, "DashBoard data retrieved successfully");
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
		/// Get Chart data for Partner Dashboard (Revenue, Orders, Commission grouped by month).
		/// </summary>
		[HttpGet("partner/{partnerId:guid}/chart")]
		public async Task<IActionResult> GetPartnerChart(
			[FromRoute] Guid partnerId,
			[FromQuery] DateTime? startDate,
			[FromQuery] DateTime? endDate)
		{
			var result = await _dashboardService.GetPartnerChartAsync(partnerId, startDate, endDate);

			return Ok(new
			{
				success = true,
				message = "Partner chart data retrieved successfully",
				data = result
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
