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
	public class ShelfTransactionRepository : GenericRepository<ShelfTransaction>, IShelfTransactionRepository
	{
		public ShelfTransactionRepository(ToyShelfDbContext context) : base(context){}
		public async Task<List<Shelf>> GetShelvesByShipmentAndType(
			Guid shipmentId,
			Guid shelfTypeId)
		{
			return await _context.ShelfTransactions
				.Include(t => t.Shelf)
				.Where(t =>
					t.ReferenceId == shipmentId &&
					t.ReferenceType == ShelfReferenceType.Shipment &&
					t.ToStatus == ShelfStatus.InTransit &&
					t.Shelf.ShelfTypeId == shelfTypeId)
				.Select(t => t.Shelf)
				.ToListAsync();
		}

		public async Task<List<Shelf>> GetShelvesByShipment(Guid shipmentId)
		{
			return await _context.ShelfTransactions
				.Where(st =>
					st.ReferenceId == shipmentId &&
					st.ReferenceType == ShelfReferenceType.Shipment &&
					st.ToStatus == ShelfStatus.InTransit)
				.Include(st => st.Shelf)
				.Select(st => st.Shelf)
				.ToListAsync();
		}
	}
}
