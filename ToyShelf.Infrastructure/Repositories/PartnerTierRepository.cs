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
	public class PartnerTierRepository : GenericRepository<PartnerTier>, IPartnerTierRepository
	{
		public PartnerTierRepository(ToyShelfDbContext context) : base(context)
		{

		}
		public async Task<bool> ExistsByNameAsync(string name)
		{
			return await _context.PartnerTiers.AnyAsync(x => x.Name == name);
		}

		public async Task<bool> ExistsByPriorityAsync(int priority)
		{
			return await _context.PartnerTiers.AnyAsync(x => x.Priority == priority);
		}

		public async Task<bool> IsTierInUseAsync(Guid id)
		{
			// Check bảng Partners xem có ai đang ở Tier này không
			return await _context.Partners.AnyAsync(p => p.PartnerTierId == id);
		}
	}
}
