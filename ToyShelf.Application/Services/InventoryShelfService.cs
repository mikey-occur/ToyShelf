using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Services
{
	public class InventoryShelfService : IInventoryShelfService
	{
		public Task AddShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity)
		{
			throw new NotImplementedException();
		}

		public Task<List<InventoryShelf>> GetShelvesByLocationAsync(Guid locationId)
		{
			throw new NotImplementedException();
		}

		public Task RemoveShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity)
		{
			throw new NotImplementedException();
		}
	}
}
