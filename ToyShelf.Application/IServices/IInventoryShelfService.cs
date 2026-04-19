using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.InventoryShelf.Response;
using ToyShelf.Application.Models.Shelf.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IInventoryShelfService
	{
		Task AddShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity);
		Task RemoveShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity);
		Task<LocationShelvesResponse?> GetShelvesByLocationAsync(Guid locationId);
		Task<List<ShelfDistributionResponse>> GetShelfDistributionsAsync(Guid shelfTypeId);
	}
}
