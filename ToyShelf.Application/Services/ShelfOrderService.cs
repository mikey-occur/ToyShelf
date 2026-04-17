using Microsoft.EntityFrameworkCore;
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

		public async Task PartnerAdminApproveAsync(Guid id, ICurrentUser currentUser)
		{
			var order = await _repository.GetByIdAsync(id);

			if (order == null)
				throw new AppException("Shelf order not found", 404);

			if (order.Status != ShelfOrderStatus.Pending)
				throw new AppException("Order is not in a state to be approved by PartnerAdmin", 400);

			order.Status = ShelfOrderStatus.PartnerApproved;
			order.PartnerAdminApprovedAt = _dateTime.UtcNow;
			order.PartnerAdminApprovedByUserId = currentUser.UserId;

			_repository.Update(order);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task ApproveAsync(Guid id, ICurrentUser currentUser)
		{
			var order = await _repository.GetByIdAsync(id);

			if (order == null)
				throw new AppException("Shelf order not found", 404);

			// Phải thông qua PartnerApproved trước
			if (order.Status != ShelfOrderStatus.PartnerApproved)
				throw new AppException("Order must be approved by PartnerAdmin first", 400);

			order.Status = ShelfOrderStatus.Approved;
			order.ApprovedAt = _dateTime.UtcNow;
			order.ApprovedByUserId = currentUser.UserId;

			_repository.Update(order);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task RejectAsync(Guid id, string? adminNote, ICurrentUser currentUser)
		{
			var order = await _repository.GetByIdAsync(id);

			if (order == null)
				throw new AppException("Order not found", 404);

			// Cho phép reject khi đang ở bước Pending hoặc PartnerApproved
			var validStates = new[] { ShelfOrderStatus.Pending, ShelfOrderStatus.PartnerApproved };
			if (!validStates.Contains(order.Status))
				throw new AppException("Order already processed and cannot be rejected", 400);

			order.Status = ShelfOrderStatus.Rejected;
			order.RejectedAt = _dateTime.UtcNow;
			order.RejectedByUserId = currentUser.UserId;
			order.AdminNote = adminNote; 

			_repository.Update(order);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<List<WarehouseMatchShelfResponse>> GetAvailableWarehousesForShelfOrder(Guid shelfOrderId)
		{
			// 1. Lấy thông tin đơn hàng kệ
			var order = await _repository.GetByIdWithItemsAsync(shelfOrderId);

			if (order == null)
				throw new AppException("Không tìm thấy đơn đặt hàng kệ", 404);

			// Chỉ cho phép đơn đã Approved hoặc giao một phần (PartiallyFulfilled)
			if (order.Status == ShelfOrderStatus.Pending)
				throw new AppException("Đơn hàng đang chờ phê duyệt, không thể tạo chuyến giao", 400);

			if (order.Status == ShelfOrderStatus.Fulfilled)
				throw new AppException("Đơn hàng này đã hoàn tất", 400);

			// 2. Xác định thành phố của Store để tìm kho lân cận
			var cityId = order.StoreLocation?.Store?.CityId
				?? throw new AppException("Thông tin vị trí cửa hàng không hợp lệ", 500);

			// 3. Tìm danh sách địa điểm kho cùng thành phố
			var warehouseLocations = await _locationRepository.GetWarehouseLocationsByCityAsync(cityId);

			if (!warehouseLocations.Any())
				return new List<WarehouseMatchShelfResponse>();

			var locationIds = warehouseLocations.Select(l => l.Id).ToList();

			// 4. Lấy tất cả kệ đang "Available" tại các kho đó
			// Dùng IQueryable để tối ưu performance
			var availableShelves = await _shelfRepository.GetQueryable()
				.Include(s => s.ShelfType)
				.Where(s => locationIds.Contains(s.InventoryLocationId) &&
							s.Status == ShelfStatus.Available)
				.ToListAsync();

			// 5. Lấy nhu cầu từ đơn hàng (Chỉ lấy món chưa giao đủ)
			var orderRequirements = order.Items
				.Where(x => x.Quantity > x.FulfilledQuantity)
				.ToList();

			if (!orderRequirements.Any())
				return new List<WarehouseMatchShelfResponse>();

			// 6. Thực hiện Matching
			var result = warehouseLocations.Select(loc =>
			{
				// Lấy danh sách kệ thực tế đang nằm tại kho này
				var locShelves = availableShelves
					.Where(s => s.InventoryLocationId == loc.Id)
					.ToList();

				var matchedItems = new List<WarehouseShelfItemResponse>();

				foreach (var req in orderRequirements)
				{
					// Đếm số lượng kệ có cùng ShelfTypeId tại kho này
					var countInStock = locShelves.Count(s => s.ShelfTypeId == req.ShelfTypeId);

					if (countInStock > 0)
					{
						matchedItems.Add(new WarehouseShelfItemResponse
						{
							// ID của dòng item trong đơn hàng (CỰC KỲ QUAN TRỌNG)
							ShelfOrderItemId = req.Id,

							ShelfTypeId = req.ShelfTypeId,
							ShelfTypeName = req.ShelfType?.Name ?? "N/A",
							ImageUrl = req.ShelfType?.ImageUrl,

							AvailableQuantity = countInStock,

							// Thông số đơn hàng để FE hiển thị gợi ý
							OriginalQuantity = req.Quantity,
							FulfilledQuantity = req.FulfilledQuantity,
							RemainingQuantity = req.Quantity - req.FulfilledQuantity
						});
					}
				}

				return new { Location = loc, Items = matchedItems };
			})
			.Where(x => x.Items.Any()) // Bỏ qua kho không có món nào mình cần
			.Select(x => new WarehouseMatchShelfResponse
			{
				WarehouseId = x.Location.WarehouseId ?? Guid.Empty,
				WarehouseLocationId = x.Location.Id,
				WarehouseName = x.Location.Warehouse?.Name ?? "N/A",
				WarehouseCode = x.Location.Warehouse?.Code ?? "N/A",
				Items = x.Items
			})
			.OrderBy(x => x.WarehouseName)
			.ToList();

			return result;
		}

		public async Task<IEnumerable<ShelfOrderResponse>> GetByPartnerAsync(Guid partnerId, ShelfOrderStatus? status)
		{
			var orders = await _repository.GetOrdersByPartnerAsync(partnerId, status);
			return orders.Select(MapToResponse);
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

				ShipmentAssignmentIds = order.AssignmentShelfOrders?
					.Select(ash => ash.ShipmentAssignmentId)
					.ToList() ?? new List<Guid>(),

				StoreLocationId = order.StoreLocationId,
				StoreName = order.StoreLocation?.Store?.Name ?? "",

				RequestedByUserId = order.RequestedByUserId,
				RequestName = order.RequestedByUser?.FullName ?? "",

				PartnerAdminApprovedByUserId = order.PartnerAdminApprovedByUserId,
				PartnerAdminName = order.PartnerAdminApprovedByUser?.FullName ?? string.Empty,

				ApprovedByUserId = order.ApprovedByUserId,
				ApproveName = order.ApprovedByUser?.FullName ?? "",

				RejectedByUserId = order.RejectedByUserId,
				RejectName = order.RejectedByUser?.FullName ?? "",

				Status = order.Status,
				Note = order.Note,
				AdminNote = order.AdminNote,

				CreatedAt = order.CreatedAt,
				PartnerAdminApprovedAt = order.PartnerAdminApprovedAt,
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
