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
	public class StoreCreationRequestRepository : GenericRepository<StoreCreationRequest>, IStoreCreationRequestRepository
	{
		public StoreCreationRequestRepository(ToyShelfDbContext context) : base(context) { }
		public async Task<IEnumerable<StoreCreationRequest>> GetRequestsAsync(StoreRequestStatus? status)
		{
			var query = _context.StoreCreationRequests
				.Include(x => x.City)
				.Include(x => x.Partner)
				.Include(x => x.RequestedByUser)
				.Include(x => x.ReviewedByUser)
				.AsQueryable();

			if (status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value);
			}

			return await query
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
		}
		public async Task<StoreCreationRequest?> GetByIdWithCityAsync(Guid id)
		{
			return await _context.StoreCreationRequests
				.Include(x => x.City)
				.Include(x => x.Partner)
				.Include(x => x.RequestedByUser)
				.Include(x => x.ReviewedByUser)
				.FirstOrDefaultAsync(x => x.Id == id);
		}
	}
}
