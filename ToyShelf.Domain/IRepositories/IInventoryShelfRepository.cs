using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IInventoryShelfRepository : IGenericRepository<InventoryShelf>
	{
		Task<InventoryShelf?> GetShelfAsync(Guid locationId, Guid shelfTypeId);
		Task<List<InventoryShelf>> GetShelvesByLocationAsync(Guid locationId);
		Task<List<InventoryShelf>> GetDistributionsByShelfTypeAsync(Guid shelfTypeId);
		Task<InventoryShelf?> GetByLocationAndTypeAsync(Guid locationId, Guid shelfTypeId);
		Task<InventoryShelf?> GetShelfWithStatusAsync(Guid locationId, Guid shelfTypeId, ShelfStatus status);
	}
}
