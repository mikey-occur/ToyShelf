using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Warehouse.Response;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DashboardController : ControllerBase
	{
		private readonly IDashboardService _dashboardService;

		public DashboardController(IDashboardService dashboardService)
		{
			_dashboardService = dashboardService;
		}






















































































		// ================= WAREHOUSE DASHBOARD =================
		[HttpGet("warehouse/{warehouseId}")]
		public async Task<BaseResponse<WarehouseDashboardResponse>> GetWarehouseDashboard(Guid warehouseId)
		{
			var result = await _dashboardService.GetWarehouseDashboard(warehouseId);

			return BaseResponse<WarehouseDashboardResponse>
				.Ok(result, "Warehouse dashboard retrieved successfully");
		}
	}
}
