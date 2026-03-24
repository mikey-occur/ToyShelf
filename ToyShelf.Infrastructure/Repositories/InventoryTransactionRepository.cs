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
	public class InventoryTransactionRepository : GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
	{
		public InventoryTransactionRepository(ToyShelfDbContext context) : base(context){}
		public async Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(Guid productId)
		{
			return await _context.InventoryTransactions
				.Include(t => t.ProductColor)
					.ThenInclude(pc => pc.Product)
				.Include(t => t.FromLocation)
				.Include(t => t.ToLocation)
				.Where(t => t.ProductColor.ProductId == productId)
				.OrderByDescending(t => t.CreatedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<InventoryTransaction>> GetAllTransactionsAsync(
		   Guid? productId = null,
		   Guid? fromLocationId = null,
		   Guid? toLocationId = null)
		{
			var query = _context.InventoryTransactions
				.Include(t => t.ProductColor)
					.ThenInclude(pc => pc.Product)
				.Include(t => t.ProductColor)
					.ThenInclude(pc => pc.Color)
				.Include(t => t.FromLocation)
				.Include(t => t.ToLocation)
				.AsQueryable();

			if (productId.HasValue)
				query = query.Where(t => t.ProductColor.ProductId == productId.Value);

			if (fromLocationId.HasValue)
				query = query.Where(t => t.FromLocationId == fromLocationId.Value);

			if (toLocationId.HasValue)
				query = query.Where(t => t.ToLocationId == toLocationId.Value);

			return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
		}
	}
}
