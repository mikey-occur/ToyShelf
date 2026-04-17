using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.StoreOrder.Request;
using ToyShelf.Application.Models.StoreOrder.Response;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IStoreOrderService
	{
		Task<StoreOrderResponse> CreateAsync(CreateStoreOrderRequest request, ICurrentUser currentUser);
		Task<IEnumerable<StoreOrderResponse>> GetAllAsync(StoreOrderStatus? status);
		Task<StoreOrderResponse> GetByIdAsync(Guid id);

		Task PartnerAdminApproveAsync(Guid id, ICurrentUser currentUser);
		Task AdminApproveAsync(Guid id, ICurrentUser currentUser);
		Task RejectAsync(Guid id, ICurrentUser currentUser);
		Task<List<WarehouseMatchResponse>> GetAvailableWarehousesAsync(Guid storeOrderId);
		Task<IEnumerable<StoreOrderResponse>> GetOrdersForAdminAsync(Guid partnerId, StoreOrderStatus? status);
	}
}
