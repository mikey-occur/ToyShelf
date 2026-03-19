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
	}
}
