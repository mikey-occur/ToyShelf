using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Dashboard.Request;
using ToyShelf.Application.Models.Dashboard.Response;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.Shipment.Response;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Application.Services;
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
			[FromQuery] DateTime? fromDate,
			[FromQuery] DateTime? toDate)
		{
			// 1. Gọi Service để kéo cái data mỏng dính lên
			var result = await _dashboardService.GetPartnerStatCardAsync(partnerId, fromDate, toDate);
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

		/// <summary>
		/// Lấy sản phẩm bán chạy nhất 
		/// </summary>
		[HttpGet("top-selling")]
		public async Task<BaseResponse<List<TopSellingProductResponse>>> GetTopSellingProducts(
			[FromQuery] int? month,
			[FromQuery] int? year,
			[FromQuery] Guid? storeId,
			[FromQuery] Guid? partnerId)
		{
			if (month.HasValue && !year.HasValue)
			{
				year = DateTime.Now.Year;
			}

			var result = await _dashboardService.GetTopSellingProductsAsync(month, year, storeId, partnerId);
			return BaseResponse<List<TopSellingProductResponse>>.Ok(result, "Lấy Top 3 bán chạy thành công");
		}

		/// <summary>
		/// Lấy danh sách Top Cửa hàng có doanh thu cao nhất (Bảng vàng)
		/// </summary>
		/// <param name="month">Tháng cần xem (từ 1 đến 12)</param>
		/// <param name="year">Năm cần xem</param>
		/// <param name="partnerId">ID đối tác (nếu muốn lọc theo đối tác)</param>
		[HttpGet("top-stores")]
		public async Task<BaseResponse<List<TopStoreResponse>>> GetTopStoresByRevenue(
			[FromQuery] int? month,
			[FromQuery] int? year, [FromQuery] Guid? partnerId)
		{
			
			if (month.HasValue && !year.HasValue)
			{
				year = DateTime.Now.Year;
			}

			// Gọi Service lấy data (Mặc định lấy top 3 như đã setup)
			var result = await _dashboardService.GetTopStoresByRevenueAsync(month, year, partnerId);
			return BaseResponse<List<TopStoreResponse>>.Ok(result, "Lấy bảng xếp hạng cửa hàng thành công!");
		}


		/// <summary>
		/// Lấy danh sách Top Đối tác có doanh thu cao nhất
		/// </summary>
		[HttpGet("top-partners")]
		public async Task<BaseResponse<List<TopPartnerResponse>>> GetTopPartners([FromQuery] int? month, [FromQuery] int? year)
		{
			// Fix chống cháy: Có tháng mà không có năm thì lấy năm hiện tại
			if (month.HasValue && !year.HasValue)
			{
				year = DateTime.Now.Year;
			}

			var result = await _dashboardService.GetTopPartnersByRevenueAsync(month, year);

			return BaseResponse<List<TopPartnerResponse>>.Ok(result, "Lấy bảng xếp hạng đối tác thành công!");
		}


		/// <summary>
		/// Lấy thông tin thống kê (Stat Card) cho Shipper đang đăng nhập
		/// </summary>
		[HttpGet("shipper/stat-card")]
		[Authorize(Roles = "Warehouse")]
		public async Task<BaseResponse<ShipperStatCardResponse>> GetShipperStatCard([FromServices] ICurrentUser currentUser)
		{
			var shipperId = currentUser.UserId;

			var result = await _dashboardService.GetShipperStatCardAsync(shipperId);

			return BaseResponse<ShipperStatCardResponse>.Ok(result, "Lấy thông tin Stat Card thành công!");
		}










































		// ================= WAREHOUSE DASHBOARD =================
		[HttpGet("warehouse/{warehouseId}")]
		public async Task<BaseResponse<WarehouseDashboardResponse>> GetWarehouseDashboard(Guid warehouseId, [FromQuery] StoreChartRequest request)
		{
			var result = await _dashboardService.GetWarehouseDashboard(warehouseId, request);

			return BaseResponse<WarehouseDashboardResponse>
				.Ok(result, "Warehouse dashboard retrieved successfully");
		}


















		///////////////////////////
	}
}
