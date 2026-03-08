using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IInventoryRepository
	{
		Task<Inventory?> GetInventoryAsync(Guid storeId, Guid productColorId, string dispositionCode);
	}
}
