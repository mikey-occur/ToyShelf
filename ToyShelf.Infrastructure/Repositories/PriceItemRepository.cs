using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Infrastructure.Repositories
{
	public class PriceItemRepository : GenericRepository<PriceItem>, IPriceItemRepository
	{
		public PriceItemRepository(Context.ToyShelfDbContext context) : base(context) { }
		
		
		public async Task<Domain.Entities.PriceItem?> GetItemAsync(Guid priceTableId, Guid segmentId)
		{
			return await _context.PriceItems
				.FirstOrDefaultAsync(pi => pi.PriceTableId == priceTableId && pi.PriceSegmentId == segmentId);
		}
	

	}

}
