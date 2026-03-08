using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Order;

namespace ToyShelf.Application.IServices
{
	public interface IOrderService
	{
		Task<string> CreateOrderAndGetPaymentLinkAsync(CreateOrderRequest request);
		Task<Guid?> HandlePaymentSuccessAsync(long orderCode);
	}
}
