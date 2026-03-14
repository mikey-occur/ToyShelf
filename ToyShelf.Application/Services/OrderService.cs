using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Order;
using ToyShelf.Application.Payment;
using ToyShelf.Domain.Common.Commission;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class OrderService : IOrderService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrderRepository _orderRepository;
		private readonly IProductColorRepository _productColorRepository;
		private readonly IServices.IPaymentService _paymentService;
		private readonly IDateTimeProvider _dateTime;
		private readonly ICommissionService _commissionService;
		private readonly ICommissionHistoryRepsitory _commissionHistoryRepository;
		private readonly IInventoryService _inventoryService;

		public OrderService(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IProductColorRepository productColorRepository, IServices.IPaymentService paymentService, IDateTimeProvider dateTime, ICommissionService commissionService, ICommissionHistoryRepsitory commissionHistoryRepsitory, IInventoryService inventoryService)
		{
			_unitOfWork = unitOfWork;
			_orderRepository = orderRepository;
			_productColorRepository = productColorRepository;
			_paymentService = paymentService;
			_dateTime = dateTime;
			_commissionService = commissionService;
			_commissionHistoryRepository = commissionHistoryRepsitory;
			_inventoryService = inventoryService;
		}
		public async Task<string> CreateOrderAndGetPaymentLinkAsync(CreateOrderRequest request)
		{
			var orderCode = long.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
			var order = new Order
			{
				Id = Guid.NewGuid(),
				StoreId = request.StoreId,
				StaffId = request.StaffId,
				OrderCode = orderCode,
				TotalAmount = 0,
				PaymentMethod = "QR",
				Status =  "CREATED", // Trạng thái chờ thanh toán
				CreatedAt = _dateTime.UtcNow,

			};

		

			foreach (var itemReq in request.Items)
			{
				// Lấy thông tin sản phẩm từ Repo để lấy giá chuẩn (không tin giá từ client)
				var product = await _productColorRepository.GetByIdAsync(itemReq.ProductColorId);

				if (product == null)
				{
					throw new Exception($"Sản phẩm với ID {itemReq.ProductColorId} không tồn tại.");
				}


				var orderItem = new OrderItem
				{
					Id = Guid.NewGuid(),
					OrderId = order.Id,
					ProductColorId = itemReq.ProductColorId,
					Quantity = itemReq.Quantity,
					Price = product.Price 
				};

				order.TotalAmount += (orderItem.Price * orderItem.Quantity);
				order.OrderItems.Add(orderItem);
			}
			// Lưu đơn hàng vào database
			await _orderRepository.AddAsync(order);
			await _unitOfWork.SaveChangesAsync();
			try
			{
				// Gọi service bạn vừa fix gạch đỏ xong
				string paymentUrl = await _paymentService.CreatePaymentLink(order);
				return paymentUrl;
			}
			catch (Exception ex )
			{
				// Nếu tạo link thất bại, có thể cập nhật trạng thái đơn hàng thành FAILED
				Console.WriteLine("---------- PAYOS API ERROR ----------");
				Console.WriteLine(ex.Message);
				if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
				Console.WriteLine("-------------------------------------");
				order.Status = "CANCELED";
				await _unitOfWork.SaveChangesAsync();
				throw new Exception("Không thể tạo link thanh toán, vui lòng thử lại.");
			}

		}

		public async Task<OrderDetailResponse?> GetOrderDetailsAsync(long orderCode)
		{
			var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderCode);
			if (order == null) throw new Exception($"Order không tồn tại."); ;
			var response = new OrderDetailResponse
			{
				Id = order.Id,
				OrderCode = order.OrderCode,
				TotalAmount = order.TotalAmount,
				PaymentMethod = order.PaymentMethod,
				Status = order.Status,
				CreatedAt = order.CreatedAt,
				StoreName = order.Store?.Name,
				Items = order.OrderItems.Select(oi => new OrderItemDetailResponse
				{
					ProductColorId = oi.ProductColorId,
					ProductName = oi.ProductColor.Product.Name,
					Sku = oi.ProductColor.Sku,
					ImageUrl = oi.ProductColor.ImageUrl,
					Price = oi.Price,
					Quantity = oi.Quantity
				}).ToList()
			};

			return response;
		}

		public async Task<Guid?> HandlePaymentSuccessAsync(long orderCode)
		{
			var order = await _orderRepository.GetOrderWithItemsAndStoreAsync(orderCode);

			// kiểm tra đơn hàng tồn tại và chưa được xử lý thanh toán thành công trước đó
			if (order == null)
				throw new Exception("Order not found");

			if (order.Status == "PAID")
			{
				// webhook duplicate -> ignore
				return order.Id;
			}

			// Xác định Partner nhận hoa hồng thông qua Store 
			var partnerId = order.Store?.Partner?.Id;
			if (partnerId == null)
			{
				throw new Exception("Not found partner for this store");
			}

			// 3. Duyệt qua từng sản phẩm để tính và lưu lịch sử hoa hồng
			foreach (var item in order.OrderItems)
			{
				// Gọi CommissionService với logic ưu tiên Bảng giá -> Tier Policy
				var result = await _commissionService.CalculateCommissionAsync(partnerId.Value, item.ProductColorId, item.Price);

				// Tạo bản ghi vào bảng CommissionHistory (Sử dụng đúng Rate và SourceDescription từ record)
				var commissionHistory = new CommissionHistory
				{
					Id = Guid.NewGuid(),
					OrderItemId = item.Id,
					PartnerId = partnerId.Value,
					AppliedRate = result.Rate, 
					CommissionAmount = (item.Price * item.Quantity) * result.Rate,
					CreatedAt = _dateTime.UtcNow,
					IsPaidOut = false
				};

				await _commissionHistoryRepository.AddAsync(commissionHistory);
			}

			// 4. Cập nhật trạng thái cuối cùng cho đơn hàng
			order.Status = "PAID";
			await _inventoryService.UpdateStockAfterPaymentAsync(order);
			// Lưu tất cả thay đổi (Status Order và CommissionHistory) 
			await _unitOfWork.SaveChangesAsync();
			return order.Id;
		}
	}
}
