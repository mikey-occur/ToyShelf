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
	public class InventoryDispositionRepository : GenericRepository<InventoryDisposition>, IInventoryDispositionRepository
	{
		public InventoryDispositionRepository(ToyShelfDbContext context) : base(context) { }
		public async Task<InventoryDisposition?> GetByCodeAsync(string code)
		{
			return await _context.InventoryDispositions
				.FirstOrDefaultAsync(x => x.Code == code);
		}
	}
}
