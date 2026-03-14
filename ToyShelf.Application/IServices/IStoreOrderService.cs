using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.StoreOrder.Request;
using ToyShelf.Application.Models.StoreOrder.Response;

namespace ToyShelf.Application.IServices
{
	public interface IStoreOrderService
	{
		Task<StoreOrderResponse> CreateAsync(CreateStoreOrderRequest request);
		Task<IEnumerable<StoreOrderResponse>> GetAllAsync();
		Task<StoreOrderResponse> GetByIdAsync(Guid id);
		Task ApproveAsync(Guid id);
		Task RejectAsync(Guid id);
	}
}
