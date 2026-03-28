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
    public class ShelfRepository : GenericRepository<Shelf>  , IShelfRepository
    {
        public ShelfRepository(ToyShelfDbContext context) : base(context)
        {
        }

		public async Task<(IEnumerable<Shelf> Items, int TotalCount)> GetShelvesPaginatedAsync(
		 int pageNumber = 1,
		 int pageSize = 10,
		 ShelfStatus? status = null,
		 Guid? partnerId = null,    
		 Guid? storeId = null)      
		{
			
			var query = _context.Shelves
				.Include(s => s.ShelfType)
					.ThenInclude(st => st.ShelfTypeLevels)
				.AsQueryable();

		
			if (status.HasValue)
				query = query.Where(s => s.Status == status.Value);
			if (partnerId.HasValue && partnerId != Guid.Empty)
				query = query.Where(s => s.PartnerId == partnerId.Value);
			if (storeId.HasValue && storeId != Guid.Empty)
				query = query.Where(s => s.StoreId == storeId.Value);

			
			var totalCount = await query.CountAsync();

		
			var items = await query
				.OrderByDescending(s => s.AssignedAt) 
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		public async Task<int> CountActiveShelvesByStoreAsync(Guid storeId)
		{
			return await _context.Shelves
				.CountAsync(s => s.StoreId == storeId && s.Status == ShelfStatus.InUse);
		}

		public async Task<Shelf?> GetByIdWithDetailsAsync(Guid id)
		{
			return await _context.Shelves
				.Include(s => s.ShelfType) 
					.ThenInclude(st => st.ShelfTypeLevels) 
				.FirstOrDefaultAsync(s => s.Id == id);
		}
	}
}
