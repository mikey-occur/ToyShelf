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
	public class PriceItemRepository : GenericRepository<CommissionItem>, ICommissionItemRepository
	{
		public PriceItemRepository(Context.ToyShelfDbContext context) : base(context) { }
		
		
		public async Task<Domain.Entities.CommissionItem?> GetItemAsync(Guid priceTableId, Guid segmentId)
		{
			return await _context.CommissionItems
				.FirstOrDefaultAsync(pi => pi.CommissionTableId == priceTableId && pi.PriceSegmentId == segmentId);
		}
	

	}

}
