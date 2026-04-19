using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IShelfOrderRepository : IGenericRepository<ShelfOrder>
	{
		Task<int> GetMaxSequenceAsync();
		Task<IEnumerable<ShelfOrder>> GetAllWithItemsAsync(ShelfOrderStatus? status, Guid? storeId, Guid? partnerId);
		Task<ShelfOrder?> GetByIdWithItemsAsync(Guid id);
		Task<IEnumerable<ShelfOrder>> GetOrdersByPartnerAsync(Guid partnerId, ShelfOrderStatus? status);
	}
}
