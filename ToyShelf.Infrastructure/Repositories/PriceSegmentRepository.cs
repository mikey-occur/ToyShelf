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
	public class PriceSegmentRepository : GenericRepository<PriceSegment>, IPriceSegmentRepository
	{
		public PriceSegmentRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<bool> ExistsByCodeAsync(string code)
		{
			return await _context.PriceSegments.AnyAsync(x => x.Code == code);
		}

		public async Task<bool> IsSegmentInUseAsync(Guid segmentId)
		{
			return await _context.ProductColors.AnyAsync(p => p.PriceSegmentId == segmentId);
		}
	}



}
