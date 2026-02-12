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
       ShelfStatus? status = null)
        {
            var query = _context.Shelves.AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status);

            var totalCount = await query.CountAsync();
       
            var items = await query
                .OrderByDescending(p => p.AssignedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

    }
}
