using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IOrderRepository : IGenericRepository<Order>
	{
		Task<Order?> GetOrderWithItemsAndStoreAsync(long orderCode);
		Task<Order?> GetOrderWithDetailsByIdAsync(long orderCode);

		Task<List<Order>> GetOrdersAsync(Guid? storeId, Guid? partnerId);
	}
}
