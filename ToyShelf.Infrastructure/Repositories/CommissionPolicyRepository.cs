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
	public class CommissionPolicyRepository : GenericRepository<CommissionPolicy>, ICommissionPolicyRepository
	{
		public CommissionPolicyRepository(ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<CommissionPolicy>> GetAllWithDetailsAsync()
		{
			return await _context.CommissionPolicies
				.Include(cp => cp.PartnerTier)
				.Include(cp => cp.PriceSegment)
				.ToListAsync();
		}

		public async Task<CommissionPolicy?> GetByTierAndSegmentAsync(Guid tierId, Guid segmentId)
		{
			return await _context.CommissionPolicies
				.Include(cp => cp.PartnerTier)
				.Include(cp => cp.PriceSegment)
				.FirstOrDefaultAsync(cp => cp.PartnerTierId == tierId && cp.PriceSegmentId == segmentId);
		}

		public async Task<IEnumerable<CommissionPolicy>> GetByTierIdAsync(Guid tierId)
		{
			return await _context.CommissionPolicies
				.Include(cp => cp.PartnerTier)
				.Include(cp => cp.PriceSegment)
				.Where(cp => cp.PartnerTierId == tierId)
				.ToListAsync();
		}
	}
}
