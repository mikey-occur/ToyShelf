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
	public class StoreInvitationRepository : GenericRepository<StoreInvitation>, IStoreInvitationRepository
	{
		public StoreInvitationRepository(ToyShelfDbContext context) : base(context) { }
		public async Task<IEnumerable<StoreInvitation>> GetAllWithUserAsync()
		{
			return await _context.StoreInvitations
				.Include(x => x.User)
				.Include(x => x.Store)
				.ToListAsync();
		}

	}
}
