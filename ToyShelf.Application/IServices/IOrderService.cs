using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Dashboard.Response;
using ToyShelf.Application.Models.Order;
using ToyShelf.Application.Models.Product.Response;

namespace ToyShelf.Application.IServices
{
	public interface IOrderService
	{
		Task<CreateOrderResponse> CreateOrderAndGetPaymentLinkAsync(CreateOrderRequest request);
		Task<Guid?> HandlePaymentSuccessAsync(long orderCode);
		Task<OrderDetailResponse?> GetOrderDetailsAsync(long orderCode);
		Task<List<OrderResponse>> GetOrdersAsync(Guid? storeId, Guid? partnerId, string? phone);
		Task<IEnumerable<OrderResponse>> GetOrdersByPhoneAsync(string phone);

	}
}
