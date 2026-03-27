using Microsoft.AspNetCore.Mvc;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Order;

namespace ToyShelf.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrderController : ControllerBase
	{

		private readonly IOrderService _orderService;
		public OrderController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		/// <summary>
		/// Get Orders with dynamic filters (StoreId, PartnerId, Phone)
		/// </summary>
		[HttpGet]
		public async Task<BaseResponse<IEnumerable<OrderResponse>>> GetOrders(
			[FromQuery] Guid? storeId,
			[FromQuery] Guid? partnerId,
			[FromQuery] string? phone) 
		{
			var result = await _orderService.GetOrdersAsync(storeId, partnerId, phone);

			return BaseResponse<IEnumerable<OrderResponse>>.Ok(result);
		}

		/// <summary>
		/// lấy detail order 
		/// </summary>
		[HttpGet("{ordercode}")]
		public async Task<BaseResponse<OrderDetailResponse?>> GetByOrderCode(long ordercode)
		{
			var result = await _orderService.GetOrderDetailsAsync(ordercode);

			return BaseResponse<OrderDetailResponse?>
				.Ok(result, "Order retrieved successfully");
		}
	}
}
