using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.StoreOrder.Request;
using ToyShelf.Application.Models.StoreOrder.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class StoreOrderService : IStoreOrderService
	{
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IInventoryLocationRepository _locationRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		private const string Prefix = "SO";

		public StoreOrderService(
			IStoreOrderRepository storeOrderRepository,
			IInventoryLocationRepository locationRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_storeOrderRepository = storeOrderRepository;
			_locationRepository = locationRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		public async Task<StoreOrderResponse> CreateAsync(CreateStoreOrderRequest request)
		{
			var location = await _locationRepository.GetByIdAsync(request.StoreLocationId);

			if (location == null)
				throw new AppException("Store location not found", 404);

			var maxNumber = await _storeOrderRepository
				.GetMaxSequenceAsync();

			var code = $"{Prefix}-{(maxNumber + 1):D5}";

			var order = new StoreOrder
			{
				Id = Guid.NewGuid(),
				Code = code,
				StoreLocationId = request.StoreLocationId,
				RequestedByUserId = request.RequestedByUserId,
				Status = StoreOrderStatus.Pending,
				CreatedAt = _dateTime.UtcNow
			};

			foreach (var item in request.Items)
			{
				if (item.Quantity <= 0)
					throw new AppException("Quantity must be greater than 0", 400);

				order.Items.Add(new StoreOrderItem
				{
					Id = Guid.NewGuid(),
					ProductColorId = item.ProductColorId,
					Quantity = item.Quantity
				});
			}

			await _storeOrderRepository.AddAsync(order);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(order);
		}

		public async Task<IEnumerable<StoreOrderResponse>> GetAllAsync(StoreOrderStatus status)
		{
			var orders = await _storeOrderRepository.GetAllWithItemsAsync(status);

			return orders.Select(MapToResponse);
		}

		public async Task<StoreOrderResponse> GetByIdAsync(Guid id)
		{
			var order = await _storeOrderRepository.GetByIdWithItemsAsync(id);

			if (order == null)
				throw new AppException("Store order not found", 404);

			return MapToResponse(order);
		}

		public async Task ApproveAsync(Guid id, ICurrentUser currentUser)
		{
			var order = await _storeOrderRepository.GetByIdAsync(id);

			if (order == null)
				throw new AppException("Order not found", 404);

			if (order.Status != StoreOrderStatus.Pending)
				throw new AppException("Order already processed", 400);

			order.Status = StoreOrderStatus.Approved;
			order.ApprovedAt = _dateTime.UtcNow;
			order.ApprovedByUserId = currentUser.UserId;

			_storeOrderRepository.Update(order);

			await _unitOfWork.SaveChangesAsync();
		}
			
		public async Task RejectAsync(Guid id, ICurrentUser currentUser)
		{
			var order = await _storeOrderRepository.GetByIdAsync(id);

			if (order == null)
				throw new AppException("Order not found", 404);

			if (order.Status != StoreOrderStatus.Pending)
				throw new AppException("Order already processed", 400);

			order.Status = StoreOrderStatus.Rejected;
			order.RejectedAt = _dateTime.UtcNow;
			order.RejectedByUserId = currentUser.UserId;

			_storeOrderRepository.Update(order);

			await _unitOfWork.SaveChangesAsync();
		}

		private static StoreOrderResponse MapToResponse(StoreOrder order)
		{
			return new StoreOrderResponse
			{
				Id = order.Id,
				Code = order.Code,
				StoreLocationId = order.StoreLocationId,
				Status = order.Status,
				CreatedAt = order.CreatedAt,
				Items = order.Items.Select(i => new StoreOrderItemResponse
				{
					ProductColorId = i.ProductColorId,
					Quantity = i.Quantity
				}).ToList()
			};
		}

	}
}
