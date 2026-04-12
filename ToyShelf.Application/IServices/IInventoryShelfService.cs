using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IInventoryShelfService
	{
		Task AddShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity);
		Task RemoveShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity);
		Task<List<InventoryShelf>> GetShelvesByLocationAsync(Guid locationId);
	}
}
