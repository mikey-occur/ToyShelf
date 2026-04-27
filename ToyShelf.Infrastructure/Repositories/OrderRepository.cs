using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;
using static ToyShelf.Domain.IRepositories.IOrderRepository;

namespace ToyShelf.Infrastructure.Repositories
{
	public class OrderRepository : GenericRepository<Order>, IOrderRepository
	{
		public OrderRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<List<Order>> GetOrdersAsync(Guid? storeId, Guid? partnerId, string? searchTerm, DateTime? date)
		{
			
			var query = _context.Orders
				.Include(o => o.Store)
				.Include(o => o.Staff)
				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.ProductColor) 
				.AsQueryable();

			if (storeId.HasValue)
			{
				query = query.Where(o => o.StoreId == storeId.Value);
			}

			if (partnerId.HasValue)
			{
				query = query.Where(o => o.Store != null && o.Store.PartnerId == partnerId.Value);
			}

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var cleanTerm = searchTerm.Trim();

                // Kiểm tra xem chuỗi nhập vào có phải là số không (để ưu tiên OrderCode)
                if (long.TryParse(cleanTerm, out long parsedOrderCode))
                {
                    // Nếu là số: Tìm chính xác OrderCode HOẶC tìm tương đối trong BankRef/Email
                    query = query.Where(o => o.OrderCode == parsedOrderCode
                                          || (o.BankReference != null && o.BankReference.Contains(cleanTerm))
                                          || (o.CustomerEmail != null && o.CustomerEmail.Contains(cleanTerm)));
                }
                else
                {
                    // Nếu là chữ: Tìm tương đối trong BankRef hoặc Email, 
                    query = query.Where(o => o.OrderCode.ToString().Contains(cleanTerm)
                                          || (o.BankReference != null && o.BankReference.Contains(cleanTerm))
                                          || (o.CustomerEmail != null && o.CustomerEmail.Contains(cleanTerm)));
                }
            
            }
            if (date.HasValue)
            {

                var utcDate = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);

                var startOfDay = utcDate;
                var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                query = query.Where(o => o.CreatedAt >= startOfDay && o.CreatedAt <= endOfDay);
            }

            return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
		}

		public async Task<IEnumerable<Order>> GetOrdersByCustomerPhoneAsync(string phone)
		{
			return await _context.Orders
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.ProductColor) 
				.Include(o => o.Store)
				.Where(o => o.CustomerEmail.Contains(phone)) 
				.OrderByDescending(o => o.CreatedAt) 
				.ToListAsync();
		}

		public async Task<Order?> GetOrderWithCommissionHistoryAsync(Guid orderId)
		{
			return await _context.Orders
				.Include(o => o.Store)
				.Include(o => o.Staff)

				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.ProductColor)
						.ThenInclude(pc => pc.Product) 

				
				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.CommissionHistories)
						.ThenInclude(ch => ch.Partner) 

				.FirstOrDefaultAsync(o => o.Id == orderId);
		}

		public async Task<Order?> GetOrderWithDetailsByCodeAsync(long orderCode)
		{
			return await _context.Orders
				.Include(o => o.Store)
				.Include(o => o.Staff)
				.Include(o => o.OrderItems)
					.ThenInclude(oi => oi.ProductColor)
						.ThenInclude(pc => pc.Product)
                .Include(o => o.OrderItems)
                       .ThenInclude(oi => oi.CommissionHistories)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
		}

		public async Task<Order?> GetOrderWithItemsAndStoreAsync(long orderCode)
		{
			return await _context.Orders
				.Include(o => o.Store)
					.ThenInclude(s => s.Partner)
						.ThenInclude(p => p.PartnerTier)
				.Include(o => o.OrderItems)
				.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
		}

		public async Task<List<DailyStatResult>> GetStoreChartDataAsync(Guid storeId, DateTime startDate, DateTime endDate)
		{
			return await _context.Orders
				.Where(o => o.StoreId == storeId
						 && o.Status.ToUpper() == "PAID"
						 && o.CreatedAt >= startDate
						 && o.CreatedAt <= endDate)
			
				.GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
				.Select(g => new DailyStatResult
				{
					Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
					TotalOrders = g.Count(),
					TotalRevenue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0m
				})
				.ToListAsync();
		}

		public async Task<(int TotalOrders, decimal TotalRevenue)> GetStoreStatsAsync(Guid storeId, DateTime? fromDate = null, DateTime? toDate = null)
		{
			var query = _context.Orders
		.Where(o => o.StoreId == storeId && o.Status.ToUpper() == "PAID")
		.AsQueryable();

		
			if (fromDate.HasValue)
			{
				query = query.Where(o => o.CreatedAt >= fromDate.Value);
			}

			
			if (toDate.HasValue)
			{
				query = query.Where(o => o.CreatedAt <= toDate.Value);
			}

			
			var totalOrders = await query.CountAsync();
			var totalRevenue = await query.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

			return (totalOrders, totalRevenue);
		}

		public async Task<(int TotalOrders, decimal TotalRevenue, int TotalPartners, int TotalStores)> GetSystemStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
		{
			var orderQuery = _context.Orders.Where(o => o.Status.ToUpper() == "PAID").AsQueryable();
			var partnerQuery = _context.Partners.AsQueryable();
			var storeQuery = _context.Stores.AsQueryable();

			if (fromDate.HasValue)
			{
				orderQuery = orderQuery.Where(o => o.CreatedAt >= fromDate.Value);
				partnerQuery = partnerQuery.Where(p => p.CreatedAt >= fromDate.Value);
				storeQuery = storeQuery.Where(s => s.CreatedAt >= fromDate.Value);
			}

			if (toDate.HasValue)
			{
				orderQuery = orderQuery.Where(o => o.CreatedAt <= toDate.Value);
				partnerQuery = partnerQuery.Where(p => p.CreatedAt <= toDate.Value);
				storeQuery = storeQuery.Where(s => s.CreatedAt <= toDate.Value);
			}

			// 3. Thực thi lấy số liệu song song (hoặc tuần tự)
			var totalOrders = await orderQuery.CountAsync();
			var totalRevenue = await orderQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
			var totalPartners = await partnerQuery.CountAsync();
			var totalStores = await storeQuery.CountAsync();

			// Trả về Tuple 4 món
			return (totalOrders, totalRevenue, totalPartners, totalStores);
		}

		public async Task<List<IOrderRepository.DailyStatResult>> GetSystemChartDataAsync(DateTime startDate, DateTime endDate)
		{
			return await _context.Orders
			.Where(o => o.CreatedAt >= startDate
					 && o.CreatedAt <= endDate
					 && o.Status.ToUpper() == "PAID")
			.GroupBy(o => o.CreatedAt.Date)
			.Select(g => new IOrderRepository.DailyStatResult
			{
				Date = g.Key,
				TotalOrders = g.Count(),

				TotalRevenue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0m
			})
			.ToListAsync();
			}

		public async Task<List<(Guid ProductId, Guid ProductColorId, string ProductName, string Sku, string? Brand, string ColorName, string? ImageUrl, decimal Price, int TotalSold)>> GetTopSellingProductsAsync(
		int top = 3,
		int? month = null,
		int? year = null,
		Guid? storeId = null,      
		Guid? partnerId = null)    
		{
			var query = _context.OrderItems
				.Where(oi => oi.Order.Status.ToUpper() == "PAID")
				.AsQueryable();

			if (year.HasValue)
				query = query.Where(oi => oi.Order.CreatedAt.Year == year.Value);

			if (month.HasValue)
				query = query.Where(oi => oi.Order.CreatedAt.Month == month.Value);

			// 1. Lọc theo Cửa hàng (Store)
			if (storeId.HasValue)
			{
				query = query.Where(oi => oi.Order.StoreId == storeId.Value);
			}

			// 2. Lọc theo Đối tác (Partner)
			if (partnerId.HasValue)
			{
				query = query.Where(oi => oi.Order.Store.PartnerId == partnerId.Value);
			}

			var rawData = await query
				.GroupBy(oi => oi.ProductColorId)
				.Select(g => new
				{
					ProductColorId = g.Key,
					TotalSold = g.Sum(oi => oi.Quantity)
				})
				.OrderByDescending(x => x.TotalSold)
				.Take(top)
				.Join(_context.ProductColors.Include(pc => pc.Product).Include(pc => pc.Color),
					  sold => sold.ProductColorId,
					  pc => pc.Id,
					  (sold, pc) => new
					  {
						  ProductId = pc.ProductId,
						  ProductColorId = sold.ProductColorId,
						  ProductName = pc.Product.Name,
						  Sku = pc.Sku,
						  Brand = pc.Product.Brand,
						  ColorName = pc.Color.Name,
						  ImageUrl = pc.ImageUrl,
						  Price = pc.Price,
						  TotalSold = sold.TotalSold
					  })
				.OrderByDescending(x => x.TotalSold)
				.ToListAsync();

			return rawData
				.Select(x => (
					x.ProductId, 
					x.ProductColorId,
					x.ProductName,
					x.Sku,
					x.Brand,
					x.ColorName,
					x.ImageUrl,
					x.Price,
					x.TotalSold
				))
				.ToList();
		}

		public async Task<List<(Guid StoreId, string StoreName, string City, string PartnerName, decimal TotalRevenue, int TotalOrders)>> GetTopStoresByRevenueAsync(int top = 3, int? month = null, int? year = null, Guid? partnerId = null)
		{
			var query = _context.Orders
			.Where(o => o.Status.ToUpper() == "PAID") 
			.AsQueryable();

				if (year.HasValue) query = query.Where(o => o.CreatedAt.Year == year.Value);
				if (month.HasValue) query = query.Where(o => o.CreatedAt.Month == month.Value);

				if (partnerId.HasValue)
				{
					query = query.Where(o => o.Store.PartnerId == partnerId.Value);
				}

			var rawData = await query
					.GroupBy(o => o.StoreId)
					.Select(g => new
					{
						StoreId = g.Key,
						TotalRevenue = g.Sum(o => o.TotalAmount), 
						TotalOrders = g.Count()                  
					})
					.OrderByDescending(x => x.TotalRevenue)      
					.Take(top)
					.Join(_context.Stores.Include(s => s.Partner),
						  stats => stats.StoreId,
						  store => store.Id,
						  (stats, store) => new
						  {
							  StoreId = stats.StoreId,
							  StoreName = store.Name,
							  City = store.City.Name,             
							  PartnerName = store.Partner.CompanyName, 
							  TotalRevenue = stats.TotalRevenue,
							  TotalOrders = stats.TotalOrders
						  })
					.OrderByDescending(x => x.TotalRevenue)
					.ToListAsync();

				return rawData
					.Select(x => (x.StoreId, x.StoreName, x.City, x.PartnerName, x.TotalRevenue, x.TotalOrders))
					.ToList();
		}

		public async Task<List<(Guid PartnerId, string CompanyName, string ContactName, string Email, string Tier, decimal TotalRevenue, decimal TotalCommission)>> GetTopPartnersByRevenueAsync(int top = 3, int? month = null, int? year = null)
		{
			var query = _context.CommissionHistories.AsQueryable();

			if (year.HasValue) query = query.Where(c => c.CreatedAt.Year == year.Value);
			if (month.HasValue) query = query.Where(c => c.CreatedAt.Month == month.Value);

			var rawData = await query
				.GroupBy(c => c.PartnerId)
				.Select(g => new
				{
					PartnerId = g.Key,
					TotalRevenue = g.Sum(c => c.SalesAmount),
					TotalCommission = g.Sum(c => c.CommissionAmount)
				})
				.OrderByDescending(x => x.TotalRevenue)
				.Take(top)
				.Join(_context.Partners,
					  stats => stats.PartnerId,
					  p => p.Id,
					  (stats, p) => new
					  {
						  PartnerId = stats.PartnerId,
						  CompanyName = p.CompanyName,

						
						  ContactName = p.Users.OrderBy(u => u.CreatedAt).Select(u => u.FullName).FirstOrDefault(),
						  Email = p.Users.OrderBy(u => u.CreatedAt).Select(u => u.Email).FirstOrDefault(),

						  Tier = p.PartnerTier.Name,
						  TotalRevenue = stats.TotalRevenue,
						  TotalCommission = stats.TotalCommission
					  })
				.OrderByDescending(x => x.TotalRevenue)
				.ToListAsync();

			return rawData
				.Select(x => (
					x.PartnerId,
					x.CompanyName,
					x.ContactName ?? "Đang cập nhật", 
					x.Email ?? "Đang cập nhật",
					x.Tier ?? "Đang cập nhật", 
					x.TotalRevenue,
					x.TotalCommission
				))
				.ToList();
		}
	}
}


