using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IOrderRepository : IGenericRepository<Order>
	{
		Task<Order?> GetOrderWithItemsAndStoreAsync(long orderCode);
		Task<Order?> GetOrderWithDetailsByCodeAsync(long orderCode);

		Task<List<Order>> GetOrdersAsync(Guid? storeId, Guid? partnerId, string? searchTerm, DateTime? date);
		Task<IEnumerable<Order>> GetOrdersByCustomerPhoneAsync(string phone);
		Task<(int TotalOrders, decimal TotalRevenue)> GetStoreStatsAsync(Guid storeId, DateTime? fromDate = null, DateTime? toDate = null);
		Task<List<DailyStatResult>> GetStoreChartDataAsync(Guid storeId, DateTime startDate, DateTime endDate);
		Task<Order?> GetOrderWithCommissionHistoryAsync(Guid orderId);
		Task<List<(Guid ProductId, Guid ProductColorId, string ProductName, string Sku, string? Brand, string ColorName, string? ImageUrl, decimal Price, int TotalSold)>> GetTopSellingProductsAsync(
		int top = 3,
		int? month = null,
		int? year = null,
		Guid? storeId = null,      
		Guid? partnerId = null);

		Task<List<(Guid StoreId, string StoreName, string City, string PartnerName, decimal TotalRevenue, int TotalOrders)>> GetTopStoresByRevenueAsync(
		int top = 3,
		int? month = null,
		int? year = null, Guid? partnerId = null);
		// Lấy tổng số liệu cho các thẻ Card (Tổng đơn, Tổng doanh thu toàn hệ thống)
		Task<(int TotalOrders, decimal TotalRevenue)> GetSystemStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
		Task<(int TotalPartners, int TotalStores)> GetSystemEntitiesCountAsync();
        // Lấy dữ liệu vẽ Biểu đồ doanh thu toàn hệ thống
        Task<List<DailyStatResult>> GetSystemChartDataAsync(DateTime startDate, DateTime endDate);
		public class DailyStatResult
		{
			public DateTime Date { get; set; }
			public int TotalOrders { get; set; }
			public decimal TotalRevenue { get; set; }
		}

		Task<List<(Guid PartnerId, string CompanyName, string ContactName, string Email, string Tier, decimal TotalRevenue, decimal TotalCommission)>> GetTopPartnersByRevenueAsync(
		int top = 3,
		int? month = null,
		int? year = null);

        Task<IEnumerable<Order>> GetExpiredOrdersAsync(DateTime timeoutTime);
    }
}
