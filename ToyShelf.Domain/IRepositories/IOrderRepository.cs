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

		Task<List<Order>> GetOrdersAsync(Guid? storeId, Guid? partnerId, string? phone);
		Task<IEnumerable<Order>> GetOrdersByCustomerPhoneAsync(string phone);
		Task<(int TotalOrders, decimal TotalRevenue)> GetStoreStatsAsync(Guid storeId, DateTime? fromDate = null, DateTime? toDate = null);
		Task<List<DailyStatResult>> GetStoreChartDataAsync(Guid storeId, DateTime startDate, DateTime endDate);
		Task<Order?> GetOrderWithCommissionHistoryAsync(Guid orderId);

		// Lấy tổng số liệu cho các thẻ Card (Tổng đơn, Tổng doanh thu toàn hệ thống)
		Task<(int TotalOrders, decimal TotalRevenue, int TotalPartners, int TotalStores)> GetSystemStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);

		// Lấy dữ liệu vẽ Biểu đồ doanh thu toàn hệ thống
		Task<List<DailyStatResult>> GetSystemChartDataAsync(DateTime startDate, DateTime endDate);
		public class DailyStatResult
		{
			public DateTime Date { get; set; }
			public int TotalOrders { get; set; }
			public decimal TotalRevenue { get; set; }
		}
	}
}
