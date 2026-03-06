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
		public async Task<IEnumerable<Store>> GetStoresAsync(bool? isActive)
		{
			var query = _context.Stores.AsQueryable();

			if (isActive.HasValue)
				query = query.Where(s => s.IsActive == isActive.Value);

			return await query
					.OrderByDescending(w => w.CreatedAt)
					.ToListAsync();
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
	}
}
