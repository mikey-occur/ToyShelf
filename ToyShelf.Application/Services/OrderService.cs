using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Order;
using ToyShelf.Application.Notifications;
using ToyShelf.Domain.Common.Commission;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using static ToyShelf.Application.Models.Order.PartnerOrderDetailResponse;

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
	    private readonly IInventoryRepository _inventoryRepository;
		private readonly IJobQueueService _jobQueueService;

		public OrderService(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IProductColorRepository productColorRepository, IServices.IPaymentService paymentService, IDateTimeProvider dateTime, ICommissionService commissionService, ICommissionHistoryRepsitory commissionHistoryRepsitory, IInventoryService inventoryService, IInventoryRepository inventoryRepository, IJobQueueService jobQueueService )
		{
			_unitOfWork = unitOfWork;
			_orderRepository = orderRepository;
			_productColorRepository = productColorRepository;
			_paymentService = paymentService;
			_dateTime = dateTime;
			_commissionService = commissionService;
			_commissionHistoryRepository = commissionHistoryRepsitory;
			_inventoryService = inventoryService;
			_inventoryRepository = inventoryRepository;
			_jobQueueService = jobQueueService;
		}
		public async Task<CreateOrderResponse> CreateOrderAndGetPaymentLinkAsync(CreateOrderRequest request)
		{
			var orderCode = long.Parse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
			var order = new Order
			{
				Id = Guid.NewGuid(),
				StoreId = request.StoreId,
				StaffId = request.StaffId,
				CustomerName = request.CustomerName,
				CustomerEmail = request.CustomerEmail,
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
					throw new AppException($"Product ID {itemReq.ProductColorId} Not Found.",404);
				}

				var inventory = await _inventoryRepository.GetInventoryAsync(request.StoreId, itemReq.ProductColorId, InventoryStatus.Available);

				if (inventory == null)
				{
					throw new AppException($"Product (ID: {itemReq.ProductColorId}) Stock Not Found.", 400);
				}

				if (inventory.Quantity < itemReq.Quantity)
				{
					throw new AppException($"Product (ID: {itemReq.ProductColorId}) Stock {inventory.Quantity} Not engouh to buy {itemReq.Quantity} ", 400);
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
				return new CreateOrderResponse
				{
					OrderCode = order.OrderCode,
					PaymentUrl = paymentUrl
				};
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
				throw new AppException("Không thể tạo link thanh toán, vui lòng thử lại.", 500);
			}

		}

		public async Task<OrderDetailResponse?> GetOrderDetailsAsync(long orderCode)
		{
			var order = await _orderRepository.GetOrderWithDetailsByCodeAsync(orderCode);
			if (order == null) throw new AppException($"Order not Exist.",404); ;
			var response = new OrderDetailResponse
			{
				Id = order.Id,
				StoreId = order.StoreId,
				CustomerName = order.CustomerName,
				CustomerEmail = order.CustomerEmail,
				BankReference = order.BankReference,
                StaffId = order.StaffId,
				StaffName = order.Staff?.FullName ?? "N/A",
				StaffEmail = order.Staff?.Email ?? "N/A",
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

		public async Task<List<OrderResponse>> GetOrdersAsync(Guid? storeId, Guid? partnerId, string? searchTerm, DateTime? date)
		{
			
			var orders = await _orderRepository.GetOrdersAsync(storeId, partnerId, searchTerm, date);

			
			var responseList = orders.Select(o => new OrderResponse
			{
				Id = o.Id,
				OrderCode = o.OrderCode,
				CustomerName = o.CustomerName,
				CustomerEmail = o.CustomerEmail,
				BankReference = o.BankReference,
                TotalAmount = o.TotalAmount,
				PaymentMethod = o.PaymentMethod,
				Status = o.Status,
				CreatedAt = o.CreatedAt,
				StoreName = o.Store?.Name 
			}).ToList();

			return responseList;
		}

		public async Task<IEnumerable<OrderResponse>> GetOrdersByEmailAsync(string Email)
		{
			var cleanEmail = Email?.Trim();

			if (string.IsNullOrEmpty(cleanEmail))
				return new List<OrderResponse>();

			var orders = await _orderRepository.GetOrdersByCustomerPhoneAsync(cleanEmail);

			
			var response = orders.Select(o => new OrderResponse
			{
				Id = o.Id,
				OrderCode = o.OrderCode, 
				CustomerName = o.CustomerName,
				CustomerEmail = o.CustomerEmail,
				BankReference = o.BankReference,		
				Status = o.Status,
				TotalAmount = o.TotalAmount,
				CreatedAt = o.CreatedAt,
				
			});

			return response;
		}

        public async Task<PartnerOrderDetailResponse?> GetPartnerOrderDetailsAsync(long orderCode)
        {
            var order = await _orderRepository.GetOrderWithDetailsByCodeAsync(orderCode);

            if (order == null)
                throw new AppException("Order not Exist.", 404);

            var response = new PartnerOrderDetailResponse
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                BankReference = order.BankReference,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                StoreName = order.Store?.Name,
                Items = order.OrderItems.Select(oi =>
                {
                    // Lấy thẳng record hoa hồng đầu tiên (vì 1 món hàng thường chỉ map 1 cục hoa hồng cho store đó)
                    var commission = oi.CommissionHistories.FirstOrDefault();

                    return new PartnerOrderItemDetailResponse
                    {
                        ProductColorId = oi.ProductColorId,
                        ProductName = oi.ProductColor.Product.Name,
                        Sku = oi.ProductColor.Sku,
                        ImageUrl = oi.ProductColor.ImageUrl,
                        Price = oi.Price,
                        Quantity = oi.Quantity,

                        // Trả về 0 nếu đơn chưa thanh toán hoặc lỗi không có hoa hồng
                        CommissionRate = commission?.AppliedRate ?? 0,
                        CommissionAmount = commission?.CommissionAmount ?? 0
                    };
                }).ToList()
            };

            
            response.TotalCommission = response.Items.Sum(i => i.CommissionAmount);

            return response;
        }

        public async Task<Guid?> HandlePaymentSuccessAsync(long orderCode, string? bankReference)
		{
			var order = await _orderRepository.GetOrderWithItemsAndStoreAsync(orderCode);

			// kiểm tra đơn hàng tồn tại và chưa được xử lý thanh toán thành công trước đó
			if (order == null)
				throw new AppException("Order not found", 404);

			if (order.Status == "PAID")
			{
				// webhook duplicate -> ignore
				return order.Id;
			}

			// Xác định Partner nhận hoa hồng thông qua Store 
			var partnerId = order.Store?.Partner?.Id;
			if (partnerId == null)
			{
				throw new AppException("Not found partner for this store", 404);
			}

			// 3. Duyệt qua từng sản phẩm để tính và lưu lịch sử hoa hồng
			foreach (var item in order.OrderItems)
			{
				// Gọi CommissionService với logic ưu tiên Bảng giá -> Tier Policy
				var result = await _commissionService.CalculateCommissionAsync(partnerId.Value, item.ProductColorId, item.Price);

				if (result.Rate == 0)
				{
					Console.WriteLine($"[CẢNH BÁO TÍNH TIỀN]: Đơn {orderCode} - Mã SP {item.ProductColorId} - {result.SourceDescription}");
				}
				var baseSalesAmount = item.Price * item.Quantity;
				// Tạo bản ghi vào bảng CommissionHistory (Sử dụng đúng Rate và SourceDescription từ record)
				var commissionHistory = new CommissionHistory
				{
					Id = Guid.NewGuid(),
					OrderItemId = item.Id,
					PartnerId = partnerId.Value,
					AppliedRate = result.Rate,
					SalesAmount = baseSalesAmount,
					CommissionAmount = (item.Price * item.Quantity) * result.Rate,
					CreatedAt = _dateTime.UtcNow,
				};

				await _commissionHistoryRepository.AddAsync(commissionHistory);
			}

            // 4. Cập nhật trạng thái cuối cùng cho đơn hàng
            order.BankReference = bankReference;
            order.Status = "PAID";
			await _inventoryService.UpdateStockAfterPaymentAsync(order);
			// Lưu tất cả thay đổi (Status Order và CommissionHistory) 
			await _unitOfWork.SaveChangesAsync();
			var targetEmail = order.CustomerEmail;

			if (!string.IsNullOrWhiteSpace(targetEmail))
			{
				
				_jobQueueService.EnqueuePaymentEmail(targetEmail, orderCode);
			}

			return order.Id;

		}
	}
}
