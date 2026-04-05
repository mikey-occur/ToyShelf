using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.ShelfOrder.Request;
using ToyShelf.Application.Models.ShelfOrder.Response;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ShelfOrderService : IShelfOrderService
	{
		private readonly IShelfOrderRepository _repository;
		private readonly IInventoryLocationRepository _locationRepository;
		private readonly IUserStoreRepository _userStoreRepository;
		private readonly IShelfOrderItemRepository _shelfOrderItemRepository;
		private readonly IShelfTypeRepository _shelfTypeRepository;
		private readonly IShelfRepository _shelfRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		private const string Prefix = "SHO";

		public ShelfOrderService(
			IShelfOrderRepository repository,
			IInventoryLocationRepository locationRepository,
			IUserStoreRepository userStoreRepository,
			IShelfOrderItemRepository shelfOrderItemRepository,
			IShelfTypeRepository shelfTypeRepository,
			IShelfRepository shelfRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_repository = repository;
			_locationRepository = locationRepository;
			_userStoreRepository = userStoreRepository;
			_shelfOrderItemRepository = shelfOrderItemRepository;
			_shelfTypeRepository = shelfTypeRepository;
			_shelfRepository = shelfRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// ================= CREATE =================
		public async Task<ShelfOrderResponse> CreateAsync(CreateShelfOrderRequest request, ICurrentUser currentUser)
		{
			var userStore = await _userStoreRepository.GetByUserIdAsync(currentUser.UserId);

			if (userStore == null || !userStore.IsActive)
				throw new AppException("User is not assigned to any store", 403);

			var location = await _locationRepository.GetStoreLocationByStoreIdAsync(userStore.StoreId);

			if (location == null)
				throw new AppException("Store inventory location not found", 404);

			var max = await _repository.GetMaxSequenceAsync();
			var code = $"{Prefix}-{(max + 1):D5}";

			var order = new ShelfOrder
			{
				Id = Guid.NewGuid(),
				Code = code,
				StoreLocationId = location.Id,
				RequestedByUserId = currentUser.UserId,
				Status = ShelfOrderStatus.Pending,
				Note = request.Note,
				CreatedAt = _dateTime.UtcNow
			};

			foreach (var item in request.Items)
			{
				if (item.Quantity <= 0)
					throw new AppException("Quantity must be greater than 0", 400);

				var shelfType = await _shelfTypeRepository.GetByIdAsync(item.ShelfTypeId);

				if (shelfType == null)
					throw new AppException("Shelf type not found", 404);

				order.Items.Add(new ShelfOrderItem
				{
					Id = Guid.NewGuid(),
					ShelfTypeId = item.ShelfTypeId,
					Quantity = item.Quantity,

					ShelfTypeName = shelfType.Name,
					ImageUrl = shelfType.ImageUrl
				});
			}


			await _repository.AddAsync(order);
			await _unitOfWork.SaveChangesAsync();

			var full = await _repository.GetByIdWithItemsAsync(order.Id);

			if (full == null)
				throw new AppException("Shelf order not found", 404);

			return MapToResponse(full);
		}

		// ================= GET =================
		public async Task<IEnumerable<ShelfOrderResponse>> GetAllAsync(ShelfOrderStatus? status)
		{
			var orders = await _repository.GetAllWithItemsAsync(status);
			return orders.Select(MapToResponse);
		}

		public async Task<ShelfOrderResponse> GetByIdAsync(Guid id)
		{
			var order = await _repository.GetByIdWithItemsAsync(id);

			if (order == null)
				throw new AppException("Shelf order not found", 404);

			return MapToResponse(order);
		}

		// ================= APPROVE =================
		public async Task ApproveAsync(Guid id, ICurrentUser currentUser)
		{
			var order = await _repository.GetByIdAsync(id);

			if (order == null)
				throw new AppException("Order not found", 404);

			if (order.Status != ShelfOrderStatus.Pending)
				throw new AppException("Order already processed", 400);

			order.Status = ShelfOrderStatus.Approved;
			order.ApprovedAt = _dateTime.UtcNow;
			order.ApprovedByUserId = currentUser.UserId;

			_repository.Update(order);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= REJECT =================
		public async Task RejectAsync(Guid id, string? adminNote, ICurrentUser currentUser)
		{
			var order = await _repository.GetByIdAsync(id);

			if (order == null)
				throw new AppException("Order not found", 404);

			if (order.Status != ShelfOrderStatus.Pending)
				throw new AppException("Order already processed", 400);

			order.Status = ShelfOrderStatus.Rejected;
			order.RejectedAt = _dateTime.UtcNow;
			order.RejectedByUserId = currentUser.UserId;
			order.AdminNote = adminNote;

			_repository.Update(order);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<List<WarehouseMatchShelfResponse>> GetAvailableWarehousesForShelfOrder(Guid shelfOrderId)
		{
			// 1. Lấy order
			var order = await _repository.GetByIdWithItemsAsync(shelfOrderId);

			if (order == null)
				throw new AppException("Shelf order not found", 404);

			if (order.Status != ShelfOrderStatus.Approved)
				throw new AppException("Order must be approved", 400);

			// 2. Lấy city của store
			if (order.StoreLocation?.Store == null)
				throw new AppException("Store not found", 500);

			var cityId = order.StoreLocation.Store.CityId;

			// 3. Lấy warehouse locations cùng city
			var warehouseLocations = await _locationRepository
				.GetWarehouseLocationsByCityAsync(cityId);

			if (!warehouseLocations.Any())
				return new List<WarehouseMatchShelfResponse>();

			var locationIds = warehouseLocations.Select(x => x.Id).ToList();

			// 4. Lấy shelves
			var shelves = _shelfRepository.GetQueryable()
				.Where(s =>
					locationIds.Contains(s.InventoryLocationId) &&
					s.Status == ShelfStatus.Available)
				.ToList();

			// 5. Map order items
			var orderItems = order.Items.ToDictionary(
				x => x.ShelfTypeId,
				x => x.Quantity
			);

			// 6. Group theo warehouse
			var result = shelves
				.Where(s => orderItems.ContainsKey(s.ShelfTypeId))
				.GroupBy(s => s.InventoryLocation)
				.Select(group =>
				{
					var first = group.First();
					var warehouse = first.InventoryLocation.Warehouse!;

					return new WarehouseMatchShelfResponse
					{
						WarehouseId = warehouse.Id,
						WarehouseLocationId = group.Key.Id,
						WarehouseName = warehouse.Name,
						WarehouseCode = warehouse.Code,

						Items = group
							.GroupBy(s => s.ShelfTypeId)
							.Select(g => new WarehouseShelfItemResponse
							{
								ShelfTypeId = g.Key,
								ShelfTypeName = g.First().ShelfType.Name,
								ImageUrl = g.First().ShelfType.ImageUrl,
								AvailableQuantity = g.Count()
							})
							.ToList()
					};
				})
				.ToList();

			return result;
		}

		// ================= FULFILL=================
		//public async Task FulfillAsync(Guid orderId)
		//{
		//	var order = await _repository.GetByIdWithItemsAsync(orderId);

		//	if (order == null)
		//		throw new AppException("Order not found", 404);

		//	if (order.Status != ShelfOrderStatus.Approved &&
		//		order.Status != ShelfOrderStatus.PartiallyFulfilled)
		//		throw new AppException("Order must be approved", 400);

		//	var storeLocationId = order.StoreLocationId;

		//	foreach (var item in order.Items)
		//	{
		//		var need = item.Quantity - item.FulfilledQuantity;
		//		if (need <= 0) continue;

		//		var shelves = _shelfRepository.GetQueryable()
		//			.Where(s =>
		//				s.ShelfTypeId == item.ShelfTypeId &&
		//				s.Status == ShelfStatus.Available)
		//			.Take(need)
		//			.ToList();

		//		foreach (var shelf in shelves)
		//		{
		//			shelf.Status = ShelfStatus.InUse;
		//			shelf.InventoryLocationId = storeLocationId;
		//			shelf.AssignedAt = _dateTime.UtcNow;
		//		}

		//		item.FulfilledQuantity += shelves.Count;
		//	}

		//	if (order.Items.All(x => x.FulfilledQuantity == x.Quantity))
		//		order.Status = ShelfOrderStatus.Fulfilled;
		//	else
		//		order.Status = ShelfOrderStatus.PartiallyFulfilled;

		//	await _unitOfWork.SaveChangesAsync();
		//}

		// ================= MAPPER =================
		private static ShelfOrderResponse MapToResponse(ShelfOrder order)
		{
			return new ShelfOrderResponse
			{
				Id = order.Id,
				Code = order.Code,
				StoreLocationId = order.StoreLocationId,
				StoreName = order.StoreLocation?.Store?.Name ?? "",

				RequestedByUserId = order.RequestedByUserId,
				RequestName = order.RequestedByUser?.FullName ?? "",

				ApprovedByUserId = order.ApprovedByUserId,
				ApproveName = order.ApprovedByUser?.FullName ?? "",

				RejectedByUserId = order.RejectedByUserId,
				RejectName = order.RejectedByUser?.FullName ?? "",

				Status = order.Status,
				Note = order.Note,
				AdminNote = order.AdminNote,

				CreatedAt = order.CreatedAt,
				ApprovedAt = order.ApprovedAt,
				RejectedAt = order.RejectedAt,

				Items = order.Items.Select(i => new ShelfOrderItemResponse
				{
					ShelfTypeId = i.ShelfTypeId,
					ShelfTypeName = i.ShelfType?.Name ?? "Unknown",
					ImageUrl = i.ImageUrl,

					Width = i.ShelfType?.Width ?? 0,
					Height = i.ShelfType?.Height ?? 0,
					Depth = i.ShelfType?.Depth ?? 0,
					TotalLevels = i.ShelfType?.TotalLevels ?? 0,

					Quantity = i.Quantity,
					FulfilledQuantity = i.FulfilledQuantity
				}).ToList()
			};
		}
	}
}
