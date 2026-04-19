using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.ShelfOrder.Request;
using ToyShelf.Application.Models.ShelfOrder.Response;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IShelfOrderService
	{
		Task<ShelfOrderResponse> CreateAsync(CreateShelfOrderRequest request, ICurrentUser currentUser);
		Task<IEnumerable<ShelfOrderResponse>> GetAllAsync(ShelfOrderStatus? status, Guid? storeId, Guid? partnerId);
		Task<ShelfOrderResponse> GetByIdAsync(Guid id);

		Task PartnerAdminApproveAsync(Guid id, ICurrentUser currentUser);
		Task ApproveAsync(Guid id, ICurrentUser currentUser);
		Task RejectAsync(Guid id, string? adminNote, ICurrentUser currentUser);
		//Task FulfillAsync(Guid orderId);
		Task<List<WarehouseMatchShelfResponse>> GetAvailableWarehousesForShelfOrder(Guid shelfOrderId);
		Task<List<WarehouseMatchShelfResponse>> GetAvailableWarehousesForShelfOrderV2(Guid shelfOrderId);
		Task<IEnumerable<ShelfOrderResponse>> GetByPartnerAsync(Guid partnerId, ShelfOrderStatus? status);
	}
}
