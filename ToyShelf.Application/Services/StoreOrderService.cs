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
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class StoreOrderService : IStoreOrderService
	{
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IInventoryLocationRepository _locationRepository;
		private readonly IUserStoreRepository _userStoreRepository;
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		private const string Prefix = "SO";

		public StoreOrderService(
			IStoreOrderRepository storeOrderRepository,
			IInventoryLocationRepository locationRepository,
			IUserStoreRepository userStoreRepository,
			IInventoryRepository inventoryRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_storeOrderRepository = storeOrderRepository;
			_locationRepository = locationRepository;
			_userStoreRepository = userStoreRepository;
			_inventoryRepository = inventoryRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		public async Task<StoreOrderResponse> CreateAsync(CreateStoreOrderRequest request, ICurrentUser currentUser)
		{

			// 1. Lấy store của user
			var userStore = await _userStoreRepository
				.GetByUserIdAsync(currentUser.UserId);

			if (userStore == null || !userStore.IsActive)
				throw new AppException("User is not assigned to any store", 403);

			// 2. Lấy InventoryLocation của store
			var location = await _locationRepository
				.GetStoreLocationByStoreIdAsync(userStore.StoreId);

			if (location == null)
				throw new AppException("Store inventory location not found", 404);

			var maxNumber = await _storeOrderRepository
				.GetMaxSequenceAsync();

			var code = $"{Prefix}-{(maxNumber + 1):D5}";

			var order = new StoreOrder
			{
				Id = Guid.NewGuid(),
				Code = code,
				StoreLocationId = location.Id,
				RequestedByUserId = currentUser.UserId,
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

			var orderFull = await _storeOrderRepository
								.GetByIdWithItemsAsync(order.Id);

			if (orderFull == null)
				throw new AppException("Store order not found", 404);

			return MapToResponse(orderFull);
		}

		public async Task<IEnumerable<StoreOrderResponse>> GetAllAsync(StoreOrderStatus? status)
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

		//public async Task<List<WarehouseMatchResponse>> GetAvailableWarehousesAsync(Guid storeOrderId)
		//{
		//	// 1. Lấy order
		//	var order = await _storeOrderRepository.GetByIdWithItemsAsync(storeOrderId);

		//	if (order == null)
		//		throw new AppException("Store order not found", 404);

		//	if (order.Status != StoreOrderStatus.Approved)
		//		throw new AppException("Order must be approved", 400);

		//	// 2. Lấy city của store
		//	if (order.StoreLocation?.Store == null)
		//		throw new AppException("Store location or store not found", 500);

		//	var cityId = order.StoreLocation.Store.CityId;


		//	// 3. Lấy warehouse locations cùng city
		//	var warehouseLocations = await _locationRepository
		//		.GetWarehouseLocationsByCityAsync(cityId);

		//	if (!warehouseLocations.Any())
		//		return new List<WarehouseMatchResponse>();

		//	var locationIds = warehouseLocations.Select(l => l.Id).ToList();

		//	// 4. Lấy inventory
		//	var inventories = await _inventoryRepository
		//		.GetByLocationIdsAsync(locationIds);

		//	// 5. Map product cần từ order
		//	var orderItems = order.Items.ToDictionary(
		//		x => x.ProductColorId,
		//		x => x.Quantity
		//	);

		//	// 6. Filter + Group
		//	var result = inventories
		//		.Where(i =>
		//			orderItems.ContainsKey(i.ProductColorId) &&
		//			i.Quantity > 0 &&
		//			i.Status == InventoryStatus.Available)
		//		.GroupBy(i => i.InventoryLocation)
		//		.Where(g => g.FirstOrDefault()?.InventoryLocation?.Warehouse != null)
		//		.Select(group =>
		//		{
		//			var first = group.First();
		//			var warehouse = first.InventoryLocation!.Warehouse!;

		//			return new WarehouseMatchResponse
		//			{
		//				WarehouseId = warehouse.Id,
		//				WarehouseLocationId = group.Key.Id,
		//				WarehouseName = warehouse.Name,
		//				WarehouseCode = warehouse.Code,

		//				Items = group.Select(i => new WarehouseItemResponse
		//				{
		//					ProductColorId = i.ProductColorId,
		//					SKU = i.ProductColor.Product.SKU,
		//					ProductName = i.ProductColor.Product.Name,
		//					Color = i.ProductColor.Color.Name,
		//					ImageUrl = i.ProductColor.ImageUrl,
		//					AvailableQuantity = i.Quantity,
		//					//RequestedQuantity = orderItems[i.ProductColorId]
		//				}).ToList()
		//			};
		//		})
		//		.ToList();

		//	return result;
		//}

		public async Task<List<WarehouseMatchResponse>> GetAvailableWarehousesAsync(Guid storeOrderId)
		{
			// 1. Lấy thông tin đơn hàng và các mục hàng bên trong
			// Đảm bảo Repository đã Include: StoreLocation.Store, Items.ProductColor.Product/Color
			var order = await _storeOrderRepository.GetByIdWithItemsAsync(storeOrderId);

			if (order == null)
				throw new AppException("Không tìm thấy đơn đặt hàng của cửa hàng", 404);

			// 2. Kiểm tra trạng thái đơn hàng
			// Cho phép: Approved (Đã duyệt) và PartiallyFulfilled (Đã giao một phần)
			// Chặn: Pending (Chờ duyệt), Fulfilled (Đã hoàn tất), Cancelled (Đã hủy)
			if (order.Status == StoreOrderStatus.Pending)
				throw new AppException("Đơn hàng đang chờ phê duyệt, không thể thực hiện giao hàng", 400);

			if (order.Status == StoreOrderStatus.Fulfilled)
				throw new AppException("Đơn hàng này đã được giao hoàn tất", 400);

			//if (order.Status == StoreOrderStatus.Cancelled)
			//	throw new AppException("Đơn hàng đã bị hủy bỏ", 400);

			// 3. Xác định khu vực (City) của Store để tìm kho gần nhất
			if (order.StoreLocation?.Store == null)
				throw new AppException("Thông tin vị trí cửa hàng không hợp lệ", 500);

			var cityId = order.StoreLocation.Store.CityId;

			// 4. Lấy danh sách các địa điểm kho cùng thành phố
			var warehouseLocations = await _locationRepository.GetWarehouseLocationsByCityAsync(cityId);

			if (!warehouseLocations.Any())
				return new List<WarehouseMatchResponse>();

			var locationIds = warehouseLocations.Select(l => l.Id).ToList();

			// 5. Lấy toàn bộ tồn kho "Sẵn có" (Available) của các kho trong danh sách
			var inventories = await _inventoryRepository.GetByLocationIdsAsync(locationIds);

			// 6. Lọc nhu cầu từ đơn hàng: Chỉ lấy những món chưa giao đủ
			var orderRequirement = order.Items
				.Where(x => x.Quantity > x.FulfilledQuantity) // Logic cực kỳ quan trọng cho Partial Fulfillment
				.Select(x => new {
					Id = x.Id,
					ProductColorId = x.ProductColorId,
					TotalRequested = x.Quantity,
					AlreadyFulfilled = x.FulfilledQuantity,
					RemainingNeeded = x.Quantity - x.FulfilledQuantity
				}).ToList();

			// Nếu tất cả các món đã giao đủ, trả về danh sách rỗng
			if (!orderRequirement.Any())
				return new List<WarehouseMatchResponse>();

			// 7. Thực hiện so khớp (Matching) giữa kho và nhu cầu
			var result = warehouseLocations.Select(loc =>
			{
				// Lấy danh sách tồn kho thực tế tại địa điểm kho này
				var locInventories = inventories
					.Where(i => i.InventoryLocationId == loc.Id && i.Status == InventoryStatus.Available)
					.ToList();

				var matchedItems = new List<WarehouseItemResponse>();

				foreach (var req in orderRequirement)
				{
					// Tìm sản phẩm trong kho này
					var inv = locInventories.FirstOrDefault(i => i.ProductColorId == req.ProductColorId);

					// Chỉ thêm vào danh sách nếu kho có hàng (Số lượng > 0)
					if (inv != null && inv.Quantity > 0)
					{
						matchedItems.Add(new WarehouseItemResponse
						{
							// ID của dòng item trong đơn hàng (Dùng để gửi lên Assignment sau này)
							StoreOrderItemId = req.Id,

							ProductColorId = inv.ProductColorId,
							SKU = inv.ProductColor?.Product?.SKU ?? "N/A",
							ProductName = inv.ProductColor?.Product?.Name ?? "N/A",
							Color = inv.ProductColor?.Color?.Name ?? "N/A",
							ImageUrl = inv.ProductColor?.ImageUrl,

							// Thông số tồn kho của kho hiện tại
							AvailableQuantity = inv.Quantity,

							// Thông số đơn hàng để FE hiển thị gợi ý cho Admin
							OriginalQuantity = req.TotalRequested,
							FulfilledQuantity = req.AlreadyFulfilled,
							RemainingQuantity = req.RemainingNeeded
						});
					}
				}

				return new { Location = loc, Items = matchedItems };
			})
			.Where(x => x.Items.Any()) // Loại bỏ những kho không chứa bất kỳ món nào mình cần
			.Select(x => new WarehouseMatchResponse
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

		private static StoreOrderResponse MapToResponse(StoreOrder order)
		{
			return new StoreOrderResponse
			{
				Id = order.Id,
				Code = order.Code,
				StoreLocationId = order.StoreLocationId,
				ShipmentAssignmentIds = order.AssignmentStoreOrders?
					.Select(aso => aso.ShipmentAssignmentId)
					.ToList() ?? new List<Guid>(),
				StoreName = order.StoreLocation?.Store?.Name ?? string.Empty,
				StoreAddress = order.StoreLocation?.Store?.StoreAddress ?? string.Empty,
				RequestedByUserId = order.RequestedByUserId,
				RequestName = order.RequestedByUser?.FullName ?? string.Empty,
				ApprovedByUserId = order.ApprovedByUserId,
				ApproveName = order.ApprovedByUser?.FullName ?? string.Empty,
				RejectedByUserId = order.RejectedByUserId,
				RejectName = order.RejectedByUser?.FullName ?? string.Empty,
				Status = order.Status,
				CreatedAt = order.CreatedAt,
				ApprovedAt = order.ApprovedAt,
				RejectedAt = order.RejectedAt,
				Items = order.Items.Select(i => new StoreOrderItemResponse
				{
					ProductColorId = i.ProductColorId,
					SKU = i.ProductColor.Product.SKU,
					ProductName = i.ProductColor.Product.Name,
					Color = i.ProductColor.Color.Name,
					ImageUrl = i.ProductColor.ImageUrl,
					Quantity = i.Quantity,
					FulfilledQuantity = i.FulfilledQuantity
				}).ToList()
			};
		}

	}
}
