using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
	public class UserStoreRepository : GenericRepository<UserStore>, IUserStoreRepository
	{
		public UserStoreRepository(ToyShelfDbContext context) : base(context) { }
		public async Task<IEnumerable<UserStore>> GetUserStoresWithStoreAsync(Guid userId)
		{
			return await _context.UserStores
				.Include(x => x.Store)
					.ThenInclude(x => x.InventoryLocations)
				.Where(x => x.UserId == userId && x.IsActive)
				.ToListAsync();
		}

		public async Task<UserStore?> GetByUserIdAsync(Guid userId)
		{
			return await _context.UserStores
				.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);
		}

		public async Task<UserStore?> GetActiveAsync(Guid userId, Guid storeId)
		{
			return await _context.UserStores
				.FirstOrDefaultAsync(x =>
					x.UserId == userId &&
					x.StoreId == storeId &&
					x.IsActive);
		}
	}
}
