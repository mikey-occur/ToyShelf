using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IStoreOrderRepository : IGenericRepository<StoreOrder>
	{
		Task<int> GetMaxSequenceAsync();
		Task<IEnumerable<StoreOrder>> GetAllWithItemsAsync(StoreOrderStatus? status, Guid? storeId,
	Guid? partnerId);
		Task<StoreOrder?> GetByIdWithItemsAsync(Guid id);
		Task<IEnumerable<StoreOrder>> GetOrdersByPartnerAsync(Guid partnerId, StoreOrderStatus? status);
	}
}
