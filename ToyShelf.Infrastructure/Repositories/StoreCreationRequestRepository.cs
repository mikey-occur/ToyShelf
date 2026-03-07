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
				.AsQueryable();

			if (status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value);
			}

			return await query
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
		}
	}
}
