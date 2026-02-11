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
	}
}
