using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Dashboard.Request;
using ToyShelf.Application.Models.Dashboard.Response;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.Shipment.Response;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class DashboardService : IDashboardService
	{
		private readonly IInventoryLocationRepository _inventoryLocationRepository;
		private readonly IShelfRepository _shelfRepository;
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IUserWarehouseRepository _userWarehouseRepository;
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IShipmentRepository _shipmentRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IPartnerRepository _partnerRepository;
		public DashboardService(
			IInventoryLocationRepository inventoryLocationRepository,
			IShelfRepository shelfRepository,
			IInventoryRepository inventoryRepository,
			IUserWarehouseRepository userWarehouseRepository,
			IStoreOrderRepository storeOrderRepository,
			IShipmentRepository shipmentRepository,
			IOrderRepository orderRepository,
			IPartnerRepository partnerRepository)
		{
			_inventoryLocationRepository = inventoryLocationRepository;
			_shelfRepository = shelfRepository;
			_inventoryRepository = inventoryRepository;
			_userWarehouseRepository = userWarehouseRepository;
			_storeOrderRepository = storeOrderRepository;
			_shipmentRepository = shipmentRepository;
			_orderRepository = orderRepository;
			_partnerRepository = partnerRepository;
		}


		public async Task<WarehouseDashboardResponse> GetWarehouseDashboard(Guid warehouseId, StoreChartRequest request)
		{
			var now = DateTime.UtcNow.Date;
			DateTime startDate;
			DateTime endDate;
			var viewType = (request?.ViewType ?? "month").ToLower();

			if (viewType == "year" && request?.Month != null) viewType = "month";

			switch (viewType)
			{
				case "week":
					int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
					startDate = now.AddDays(-1 * diff).Date;
					startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
					endDate = startDate.AddDays(7).AddTicks(-1);
					endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
					break;

				case "month":
					int m = request?.Month ?? now.Month;
					int y = request?.Year ?? now.Year;
					startDate = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = startDate.AddMonths(1).AddTicks(-1);
					break;

				case "year":
				default:
					int year = request?.Year ?? now.Year;
					startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
					break;
			}

			// ====================================================================
			// 2. STAT CARD TĨNH (KHÔNG bị ảnh hưởng bởi Filter ngày)
			// ====================================================================
			var totalShelves = await _shelfRepository.GetQueryable()
				.CountAsync(x => x.InventoryLocation.WarehouseId == warehouseId);

			var totalInventory = await _inventoryRepository.GetQueryable()
				.Where(x => x.InventoryLocation.WarehouseId == warehouseId)
				.SumAsync(x => (int?)x.Quantity) ?? 0;

			var totalEmployees = await _userWarehouseRepository.GetQueryable()
				.CountAsync(x => x.WarehouseId == warehouseId);

			// ====================================================================
			// 3. STAT CARD ĐỘNG (BỊ ảnh hưởng bởi startDate và endDate)
			// ====================================================================

			// Ép Filter thời gian ngay từ gốc Shipment Query
			var shipmentQuery = _shipmentRepository.GetQueryable()
				.Where(x => x.FromLocation.WarehouseId == warehouseId
						 && x.CreatedAt >= startDate
						 && x.CreatedAt <= endDate);

			// Lấy Total Orders thông qua Shipment đã được lọc
			var totalOrders = await shipmentQuery
				.SelectMany(s => s.ShipmentAssignment.AssignmentStoreOrders)
				.Select(aso => aso.StoreOrderId)
				.Distinct()
				.CountAsync();

			var totalInProgress = await shipmentQuery.CountAsync(x =>
					x.Status == ShipmentStatus.Draft ||
					x.Status == ShipmentStatus.Shipping ||
					x.Status == ShipmentStatus.Delivered
				);

			var totalCompleted = await shipmentQuery.CountAsync(x =>
				x.Status == ShipmentStatus.Completed
			);

			// Chart Shipment (Dựa trên query đã lọc)
			var shipmentChart = await shipmentQuery
				.GroupBy(x => x.Status)
				.Select(g => new ChartItem
				{
					Label = g.Key.ToString(),
					Value = g.Count()
				})
				.ToListAsync();

			// Chart Order: Kẹp thêm điều kiện CreatedAt
			var orderChart = await _storeOrderRepository.GetQueryable()
				.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
				.Where(o => o.AssignmentStoreOrders
					.Any(aso => aso.ShipmentAssignment.WarehouseLocationId == warehouseId))
				.GroupBy(o => o.Status)
				.Select(g => new ChartItem
				{
					Label = g.Key.ToString(),
					Value = g.Count()
				})
				.ToListAsync();

			return new WarehouseDashboardResponse
			{
				TotalOrders = totalOrders,
				TotalShelves = totalShelves,
				TotalInventory = totalInventory,
				TotalEmployees = totalEmployees,
				TotalInProgressShipments = totalInProgress,
				TotalCompletedShipments = totalCompleted,
				ShipmentChart = shipmentChart,
				OrderChart = orderChart
			};
		}

		private (DateTime startDate, DateTime endDate) BuildDateRange(StoreChartRequest request)
		{
			var now = DateTime.UtcNow.Date;
			DateTime startDate;
			DateTime endDate;

			var viewType = (request?.ViewType ?? "month").ToLower();

			// tránh conflict logic cũ
			if (viewType == "year" && request?.Month != null)
				viewType = "month";

			switch (viewType)
			{
				case "day":
					var date = request?.Date?.Date ?? now;

					startDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
					endDate = startDate.AddDays(1).AddTicks(-1);
					break;

				case "week":
					int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;

					startDate = now.AddDays(-1 * diff).Date;
					startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

					endDate = startDate.AddDays(7).AddTicks(-1);
					endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
					break;

				case "month":
					int m = request?.Month ?? now.Month;
					int y = request?.Year ?? now.Year;

					startDate = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = startDate.AddMonths(1).AddTicks(-1);
					break;

				case "year":
				default:
					int year = request?.Year ?? now.Year;

					startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
					break;
			}

			return (startDate, endDate);
		}


        public async Task<WarehouseStatCardResponse> GetWarehouseStatCardAsync(Guid warehouseId, DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.UtcNow;

            // Chuẩn hóa thời gian
            var currentEndDate = endDate ?? now;
            var currentStartDate = startDate ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            if (currentStartDate > currentEndDate)
            {
                (currentStartDate, currentEndDate) = (currentEndDate, currentStartDate);
            }

            // Các truy vấn dữ liệu sử dụng currentStartDate và currentEndDate
            var totalShelves = await _shelfRepository.GetQueryable()
                .CountAsync(x => x.InventoryLocation.WarehouseId == warehouseId);

            var totalInventory = await _inventoryRepository.GetQueryable()
                .Where(x => x.InventoryLocation.WarehouseId == warehouseId)
                .SumAsync(x => (int?)x.Quantity) ?? 0;

            var totalEmployees = await _userWarehouseRepository.GetQueryable()
                .CountAsync(x => x.WarehouseId == warehouseId);

            var shipmentQuery = _shipmentRepository.GetQueryable()
                .Where(x => x.FromLocation.WarehouseId == warehouseId
                         && x.CreatedAt >= currentStartDate
                         && x.CreatedAt <= currentEndDate);

            var totalOrders = await shipmentQuery
                .SelectMany(s => s.ShipmentAssignment.AssignmentStoreOrders)
                .Select(aso => aso.StoreOrderId)
                .Distinct()
                .CountAsync();

            var totalInProgress = await shipmentQuery.CountAsync(x =>
                x.Status == ShipmentStatus.Draft ||
                x.Status == ShipmentStatus.Shipping ||
                x.Status == ShipmentStatus.Delivered
            );

            var totalCompleted = await shipmentQuery.CountAsync(x =>
                x.Status == ShipmentStatus.Completed
            );

            return new WarehouseStatCardResponse
            {
                WarehouseId = warehouseId,
                StartDate = currentStartDate, 
                EndDate = currentEndDate,     
                TotalOrders = totalOrders,
                TotalShelves = totalShelves,
                TotalInventory = totalInventory,
                TotalEmployees = totalEmployees,
                TotalInProgressShipments = totalInProgress,
                TotalCompletedShipments = totalCompleted
            };
        }

        public async Task<WarehouseChartResponse> GetWarehouseChartAsync(
			Guid warehouseId,
			StoreChartRequest request)
		{
			var (startDate, endDate) = BuildDateRange(request);

			var shipmentQuery = _shipmentRepository.GetQueryable()
				.Where(x => x.FromLocation.WarehouseId == warehouseId
						 && x.CreatedAt >= startDate
						 && x.CreatedAt <= endDate);

			var shipmentChart = await shipmentQuery
				.GroupBy(x => x.Status)
				.Select(g => new ChartItem
				{
					Label = g.Key.ToString(),
					Value = g.Count()
				})
				.ToListAsync();

			var orderChart = await _storeOrderRepository.GetQueryable()
				.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
				.Where(o => o.AssignmentStoreOrders
					.Any(aso => aso.ShipmentAssignment.WarehouseLocationId == warehouseId))
				.GroupBy(o => o.Status)
				.Select(g => new ChartItem
				{
					Label = g.Key.ToString(),
					Value = g.Count()
				})
				.ToListAsync();

			return new WarehouseChartResponse
			{
				ShipmentChart = shipmentChart,
				OrderChart = orderChart
			};
		}


		public async Task<StoreDashboardResponse> GetStoreRevenueAsync(Guid storeId, DateTime? fromDate = null, DateTime? toDate = null)
		{


			var (totalOrders, totalRevenue) = await _orderRepository.GetStoreStatsAsync(storeId, fromDate, toDate);

			return new StoreDashboardResponse
			{
				StoreId = storeId,
				TotalOrders = totalOrders,
				TotalRevenue = totalRevenue,
				FromDate = fromDate,
				ToDate = toDate
			};
		}

		public async Task<StoreInventoryDashboardResponse> GetStoreInventoryStatsAsync(Guid storeId)
		{
			var (totalShelves, totalProducts) =
				await _inventoryRepository.GetStoreInventoryStatsAsync(storeId);

			return new StoreInventoryDashboardResponse
			{
				StoreId = storeId,
				TotalShelves = totalShelves,
				TotalProducts = totalProducts
			};
		}

        public async Task<PartnerStatCardResponse> GetPartnerStatCardAsync(Guid partnerId, DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.UtcNow;


            var currentEndDate = endDate ?? now;
            var currentStartDate = startDate ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            if (currentStartDate > currentEndDate)
            {
                (currentStartDate, currentEndDate) = (currentEndDate, currentStartDate);
            }


            var stats = await _partnerRepository.GetPartnerStatsByDateAsync(partnerId, currentStartDate, currentEndDate);


            return new PartnerStatCardResponse
            {
                PartnerId = partnerId,
                Revenue = stats.Revenue,
                Orders = stats.Orders,
                Commission = stats.Commission,
            };
        }



        public async Task<List<StoreChartItemResponse>> GetStoreRevenueChartAsync(Guid storeId, StoreChartRequest request)
		{
			var now = DateTime.UtcNow.Date;
			DateTime startDate;
			DateTime endDate;
			var viewType = request.ViewType.ToLower();

			// Nếu ViewType là Year mà FE lại truyền kèm Month -> Mình tự bẻ lái nó thành ViewType = Month luôn
			if (viewType == "year" && request.Month.HasValue)
			{
				viewType = "month";
			}

			// 1. TÍNH TOÁN NGÀY BẮT ĐẦU VÀ KẾT THÚC DỰA VÀO FILTER
			switch (viewType)
			{
				case "week":
					int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
					startDate = now.AddDays(-1 * diff).Date;
					startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
					endDate = startDate.AddDays(7).AddTicks(-1);
					endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
					break;

				case "month":
					int m = request.Month ?? now.Month;
					int y = request.Year ?? now.Year;
					startDate = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = startDate.AddMonths(1).AddTicks(-1);
					break;

				case "year":
				default:
					int year = request.Year ?? now.Year;
					startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
					break;
			}

			// 2. KÉO DATA TỪ DB LÊN (Đã được gom theo ngày)
			var dbData = await _orderRepository.GetStoreChartDataAsync(storeId, startDate, endDate);
			var chartData = new List<StoreChartItemResponse>();

			// 3. VÒNG LẶP ĐIỀN DATA (TRÁNH BỊ GÃY BIỂU ĐỒ)
			if (viewType == "year")
			{
				// Vòng lặp 12 tháng
				for (int i = 1; i <= 12; i++)
				{
					// Lọc ra các ngày thuộc tháng i và tính tổng
					var monthData = dbData.Where(d => d.Date.Month == i).ToList();
					chartData.Add(new StoreChartItemResponse
					{
						DateLabel = $"Tháng {i}", // Trục X
						TotalOrders = monthData.Sum(x => x.TotalOrders),
						TotalRevenue = monthData.Sum(x => x.TotalRevenue)
					});
				}
			}
			else if (viewType == "week")
			{
				// Vòng lặp 7 ngày trong tuần
				var daysOfWeek = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN" };
				for (int i = 0; i < 7; i++)
				{
					var loopDate = startDate.AddDays(i);
					var dayData = dbData.FirstOrDefault(d => d.Date == loopDate);

					chartData.Add(new StoreChartItemResponse
					{
						DateLabel = daysOfWeek[i], 
						TotalOrders = dayData?.TotalOrders ?? 0,
						TotalRevenue = dayData?.TotalRevenue ?? 0m
					});
				}
			}
			else // month
			{
				// Vòng lặp từ mùng 1 đến mùng 28/29/30/31 của tháng đó
				int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
				for (int i = 1; i <= daysInMonth; i++)
				{
					var loopDate = new DateTime(startDate.Year, startDate.Month, i);
					var dayData = dbData.FirstOrDefault(d => d.Date == loopDate);

					chartData.Add(new StoreChartItemResponse
					{
						DateLabel = loopDate.ToString("dd/MM"), // Nhãn trục X: 01/03, 02/03...
						TotalOrders = dayData?.TotalOrders ?? 0,
						TotalRevenue = dayData?.TotalRevenue ?? 0m
					});
				}
			}

			return chartData;
		}

		public async Task<List<PartnerChartItemResponse>> GetPartnerChartAsync(Guid partnerId, PartnerChartRequest request)
		{
			var now = DateTime.UtcNow.Date;
			DateTime startDate;
			DateTime endDate;
			var viewType = request.ViewType?.ToLower() ?? "month";

			// Nếu ViewType là Year mà FE lại truyền kèm Month -> Mình tự bẻ lái nó thành ViewType = Month luôn
			if (viewType == "year" && request.Month.HasValue)
			{
				viewType = "month";
			}

			// 1. TÍNH TOÁN NGÀY BẮT ĐẦU VÀ KẾT THÚC DỰA VÀO FILTER
			switch (viewType)
			{
				case "week":
					int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
					startDate = now.AddDays(-1 * diff).Date;
					startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
					endDate = startDate.AddDays(7).AddTicks(-1);
					endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
					break;

				case "month":
					int m = request.Month ?? now.Month;
					int y = request.Year ?? now.Year;
					startDate = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = startDate.AddMonths(1).AddTicks(-1);
					break;

				case "year":
				default:
					int year = request.Year ?? now.Year;
					startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
					break;
			}

			// 2. KÉO DATA TỪ DB LÊN (Bắt buộc DB phải trả về Group theo Ngày)
			var dbData = await _partnerRepository.GetPartnerChartDataAsync(partnerId, startDate, endDate);
			var chartData = new List<PartnerChartItemResponse>();

			// 3. VÒNG LẶP ĐIỀN DATA (TRÁNH BỊ GÃY BIỂU ĐỒ)
			if (viewType == "year")
			{
				// Vòng lặp 12 tháng
				for (int i = 1; i <= 12; i++)
				{
					// Lọc ra các ngày thuộc tháng i và tính tổng (Roll up từ ngày lên tháng)
					var monthData = dbData.Where(d => d.Date.Month == i).ToList();
					chartData.Add(new PartnerChartItemResponse
					{
						DateLabel = $"Tháng {i}", // Trục X
						TotalOrders = monthData.Sum(x => x.TotalOrders),
						TotalRevenue = monthData.Sum(x => x.TotalRevenue),
						TotalCommission = monthData.Sum(x => x.TotalCommission) // Thêm Commission
					});
				}
			}
			else if (viewType == "week")
			{
				// Vòng lặp 7 ngày trong tuần
				var daysOfWeek = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN" };
				for (int i = 0; i < 7; i++)
				{
					var loopDate = startDate.AddDays(i);
					var dayData = dbData.FirstOrDefault(d => d.Date == loopDate);

					chartData.Add(new PartnerChartItemResponse
					{
						DateLabel = daysOfWeek[i],
						TotalOrders = dayData?.TotalOrders ?? 0,
						TotalRevenue = dayData?.TotalRevenue ?? 0m,
						TotalCommission = dayData?.TotalCommission ?? 0m // Thêm Commission
					});
				}
			}
			else // month
			{
				// Vòng lặp từ mùng 1 đến ngày cuối của tháng đó
				int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
				for (int i = 1; i <= daysInMonth; i++)
				{
					var loopDate = new DateTime(startDate.Year, startDate.Month, i);
					var dayData = dbData.FirstOrDefault(d => d.Date == loopDate);

					chartData.Add(new PartnerChartItemResponse
					{
						DateLabel = loopDate.ToString("dd/MM"), // Nhãn trục X
						TotalOrders = dayData?.TotalOrders ?? 0,
						TotalRevenue = dayData?.TotalRevenue ?? 0m,
						TotalCommission = dayData?.TotalCommission ?? 0m // Thêm Commission
					});
				}
			}

			return chartData;
		}

		public async Task<SystemStatsResponse> GetSystemStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
		{
			var stats = await _orderRepository.GetSystemStatsAsync(fromDate, toDate);

			return new SystemStatsResponse
			{
				TotalOrders = stats.TotalOrders,
				TotalRevenue = stats.TotalRevenue,
			};
		}

		public async Task<List<SystemChartItemResponse>> GetSystemRevenueChartAsync(StoreChartRequest request)
		{
			var now = DateTime.UtcNow.Date;
			DateTime startDate;
			DateTime endDate;
			var viewType = request.ViewType.ToLower();

			// Bẻ lái logic nếu gửi sai
			if (viewType == "year" && request.Month.HasValue)
			{
				viewType = "month";
			}

			switch (viewType)
			{
				case "week":
					int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
					startDate = now.AddDays(-1 * diff).Date;
					startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
					endDate = startDate.AddDays(7).AddTicks(-1);
					endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
					break;

				case "month":
					int m = request.Month ?? now.Month;
					int y = request.Year ?? now.Year;
					startDate = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = startDate.AddMonths(1).AddTicks(-1);
					break;

				case "year":
				default:
					int year = request.Year ?? now.Year;
					startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
					endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
					break;
			}

			
			var dbData = await _orderRepository.GetSystemChartDataAsync(startDate, endDate);
			var chartData = new List<SystemChartItemResponse>();

		
			if (viewType == "year")
			{
				for (int i = 1; i <= 12; i++)
				{
					var monthData = dbData.Where(d => d.Date.Month == i).ToList();
					chartData.Add(new SystemChartItemResponse
					{
						DateLabel = $"Tháng {i}",
						TotalOrders = monthData.Sum(x => x.TotalOrders),
						TotalRevenue = monthData.Sum(x => x.TotalRevenue)
					});
				}
			}
			else if (viewType == "week")
			{
				var daysOfWeek = new[] { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "CN" };
				for (int i = 0; i < 7; i++)
				{
					var loopDate = startDate.AddDays(i);
					var dayData = dbData.FirstOrDefault(d => d.Date == loopDate);

					chartData.Add(new SystemChartItemResponse
					{
						DateLabel = daysOfWeek[i],
						TotalOrders = dayData?.TotalOrders ?? 0,
						TotalRevenue = dayData?.TotalRevenue ?? 0m
					});
				}
			}
			else // month
			{
				int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
				for (int i = 1; i <= daysInMonth; i++)
				{
					var loopDate = new DateTime(startDate.Year, startDate.Month, i);
					var dayData = dbData.FirstOrDefault(d => d.Date == loopDate);

					chartData.Add(new SystemChartItemResponse
					{
						DateLabel = loopDate.ToString("dd/MM"),
						TotalOrders = dayData?.TotalOrders ?? 0,
						TotalRevenue = dayData?.TotalRevenue ?? 0m
					});
				}
			}

			return chartData;
		}

		public async Task<List<TopSellingProductResponse>> GetTopSellingProductsAsync(int? month = null, int? year = null, Guid? storeId = null, Guid? partnerId = null)
		{
			var tupleList = await _orderRepository.GetTopSellingProductsAsync(3, month, year, storeId, partnerId);

			var response = tupleList.Select(t => new TopSellingProductResponse
			{
				ProductId = t.ProductId,
				ProductColorId = t.ProductColorId,
				ProductName = t.ProductName,
				Price = t.Price,
				TotalSold = t.TotalSold,
				Brand = t.Brand,
				Sku = t.Sku,
				ColorName = t.ColorName,
				ImageUrl = t.ImageUrl
			}).ToList();

			return response;
		}

		public async Task<List<TopStoreResponse>> GetTopStoresByRevenueAsync(int? month = null, int? year = null, Guid? partnerId = null)
		{
			var tupleList = await _orderRepository.GetTopStoresByRevenueAsync(3, month, year, partnerId);

			var response = tupleList.Select(t => new TopStoreResponse
			{
				StoreId = t.StoreId,
				StoreName = t.StoreName,
				City = t.City, 
				PartnerName = t.PartnerName,
				TotalRevenue = t.TotalRevenue,
				TotalOrders = t.TotalOrders
			}).ToList();

			return response;
		}

		public async Task<List<TopPartnerResponse>> GetTopPartnersByRevenueAsync(int? month = null, int? year = null)
		{
			var tupleList = await _orderRepository.GetTopPartnersByRevenueAsync(3, month, year);

			var response = tupleList.Select(t => new TopPartnerResponse
			{
				PartnerId = t.PartnerId,
				CompanyName = t.CompanyName,
				ContactName = t.ContactName ?? "Đang cập nhật",
				Email = t.Email ?? "Đang cập nhật",
				Tier = t.Tier,
				TotalRevenue = t.TotalRevenue,
				TotalCommission = t.TotalCommission
			}).ToList();

			return response;
		}

		public async Task<ShipperStatCardResponse> GetShipperStatCardAsync(Guid shipperId)
		{
			var statsTuple = await _shipmentRepository.GetShipperStatsAsync(shipperId);
		
			return new ShipperStatCardResponse
			{
				TotalDelivering = statsTuple.TotalDelivering,
				TotalCompleted = statsTuple.TotalCompleted,
				TotalCancelled = statsTuple.TotalCancelled,
				TotalAll = statsTuple.TotalAll
			};
		}

        public async Task<int> GetPartnerStoreCountAsync(Guid partnerId)
        {
            return await _partnerRepository.GetTotalStoresByPartnerAsync(partnerId);
        }

        public async Task<object> GetSystemStartCardCountAsync()
        {
            var counts = await _orderRepository.GetSystemEntitiesCountAsync();

          
            return new
            {
                TotalPartners = counts.TotalPartners,
                TotalStores = counts.TotalStores
            };
        }
    }
}
