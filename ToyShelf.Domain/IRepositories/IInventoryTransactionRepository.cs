using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IInventoryTransactionRepository : IGenericRepository<InventoryTransaction>
	{
		Task<IEnumerable<InventoryTransaction>> GetByProductIdAsync(Guid productId);
		Task<IEnumerable<InventoryTransaction>> GetAllTransactionsAsync(
		   Guid? productId = null,
		   Guid? fromLocationId = null,
		   Guid? toLocationId = null);
	}
}
