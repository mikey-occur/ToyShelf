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
		public OrderService(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IProductColorRepository productColorRepository, IServices.IPaymentService paymentService, IDateTimeProvider dateTime, ICommissionService commissionService)
		{
			_unitOfWork = unitOfWork;
			_orderRepository = orderRepository;
			_productColorRepository = productColorRepository;
			_paymentService = paymentService;
			_dateTime = dateTime;
			_commissionService = commissionService;
		}
		public async Task<string> CreateOrderAndGetPaymentLinkAsync(CreateOrderRequest request)
		{
			long orderCode = long.Parse(DateTimeOffset.Now.ToString("yyMMddHHmmss"));
			var order = new Order
			{
				Id = Guid.NewGuid(),
				StoreId = request.StoreId,
				StaffId = request.StaffId,
				OrderCode = orderCode,
				TotalAmount = request.Items.Sum(x => x.Price * x.Quantity),
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
			catch (Exception)
			{
				// Nếu tạo link thất bại, có thể cập nhật trạng thái đơn hàng thành FAILED
				order.Status = "CANCELED";
				await _unitOfWork.SaveChangesAsync();
				throw new Exception("Không thể tạo link thanh toán, vui lòng thử lại.");
			}

		}

		public async Task HandlePaymentSuccessAsync(long orderCode)
		{
			var order = await _orderRepository.GetOrderWithItemsAndStoreAsync(orderCode);

			// kiểm tra đơn hàng tồn tại và chưa được xử lý thanh toán thành công trước đó
			if (order == null || order.Status == "PAID") return;

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
					CommissionAmount = (item.Price * item.Quantity) * (result.Rate / 100),
					CreatedAt = _dateTime.UtcNow,
					IsPaidOut = false
				};

				item.CommissionHistories.Add(commissionHistory);
			}

			// 4. Cập nhật trạng thái cuối cùng cho đơn hàng
			order.Status = "PAID";

			// Lưu tất cả thay đổi (Status Order và CommissionHistory) 
			await _unitOfWork.SaveChangesAsync();
		}
	}
}
