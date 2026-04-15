using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Dashboard.Request;
using ToyShelf.Application.Models.Dashboard.Response;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.Warehouse.Response;

namespace ToyShelf.Application.IServices
{
	public interface IDashboardService
	{
		Task<WarehouseDashboardResponse> GetWarehouseDashboard(Guid warehouseId);
		Task<StoreDashboardResponse> GetStoreRevenueAsync(Guid storeId, DateTime? fromDate = null, DateTime? toDate = null);
		Task<List<PartnerChartItemResponse>> GetPartnerChartAsync(Guid partnerId, PartnerChartRequest request);
		Task<PartnerStatCardResponse> GetPartnerStatCardAsync(Guid partnerId, DateTime? startDate, DateTime? endDate);
		Task<List<StoreChartItemResponse>> GetStoreRevenueChartAsync(Guid storeId, StoreChartRequest request);

		Task<SystemStatsResponse> GetSystemStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
		Task<List<SystemChartItemResponse>> GetSystemRevenueChartAsync(StoreChartRequest request);
		Task<List<TopSellingProductResponse>> GetTopSellingProductsAsync(int? month = null, int? year = null, Guid? storeId = null, Guid? partnerId = null);
		Task<List<TopStoreResponse>> GetTopStoresByRevenueAsync(int? month = null, int? year = null, Guid? partnerId = null);
		Task<List<TopPartnerResponse>> GetTopPartnersByRevenueAsync(int? month = null, int? year = null);
	}
}
