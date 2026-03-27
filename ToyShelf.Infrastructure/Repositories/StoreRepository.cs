using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
	public class StoreRepository: GenericRepository<Store>, IStoreRepository
	{
		public StoreRepository(ToyShelfDbContext context) : base(context) { }
		public async Task<IEnumerable<Store>> GetStoresAsync(
			bool? isActive,
			Guid? companyId = null,
			string? keyword = null,
			Guid? cityId = null)
		{
			var query = _context.Stores
				.Include(s => s.InventoryLocations)
				.Include(s => s.City)
				.Include(s => s.Partner)
					.ThenInclude(p => p.Users)
						.ThenInclude(u => u.Accounts)
							.ThenInclude(a => a.AccountRoles)
								.ThenInclude(ar => ar.Role)
				.AsQueryable();

			if (isActive.HasValue)
				query = query.Where(s => s.IsActive == isActive.Value);


			if (companyId.HasValue) // giờ nó là companyId
			{
				query = query.Where(s => s.PartnerId == companyId.Value);
			}

			// filter theo khu vực (address hoặc name)
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(s =>
					s.Name.Contains(keyword) ||
					s.StoreAddress.Contains(keyword));
			}

			// filter theo city
			if (cityId.HasValue)
			{
				query = query.Where(s => s.CityId == cityId.Value);
			}

			return await query
				.OrderByDescending(s => s.CreatedAt)
				.ToListAsync();
		}

		public async Task<Store?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.Stores
				.Include(s => s.InventoryLocations)
				.Include(s => s.City)
				.Include(s => s.Partner)
					.ThenInclude(p => p.Users)
						.ThenInclude(u => u.Accounts)
							.ThenInclude(a => a.AccountRoles)
								.ThenInclude(ar => ar.Role)
				.FirstOrDefaultAsync(s => s.Id == id);
		}

		public async Task<int> GetMaxSequenceByPartnerAsync(Guid partnerId)
		{
			var codes = await _context.Stores
				.Where(s => s.PartnerId == partnerId)
				.Select(s => s.Code)
				.ToListAsync();

			if (!codes.Any())
				return 0;

			return codes
				.Select(c => int.Parse(c.Split('-').Last()))
				.Max();
		}

		public async Task<bool> ExistsByCodeInPartnerAsync(string code, Guid partnerId)
		{
			return await _context.Stores
				.AnyAsync(s => s.Code == code && s.PartnerId == partnerId);
		}

		public async Task<Store?> GetStoreWithTierAsync(Guid storeId)
		{
			return await _context.Stores
				.Include(s => s.Partner)
					.ThenInclude(p => p.PartnerTier)
				.FirstOrDefaultAsync(s => s.Id == storeId);
		}
	}
}
