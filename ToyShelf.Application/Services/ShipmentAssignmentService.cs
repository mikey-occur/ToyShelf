using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ShipmentAssignmentService : IShipmentAssignmentService
	{
		private readonly IShipmentAssignmentRepository _assignmentRepository;
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IUserWarehouseRepository _userWarehouseRepository;
		private readonly IShelfOrderRepository _repositoryShelfOrder;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public ShipmentAssignmentService(
			IShipmentAssignmentRepository assignmentRepository,
			IStoreOrderRepository storeOrderRepository,
			IUserWarehouseRepository userWarehouseRepository,
			IShelfOrderRepository repositoryShelfOrder,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_assignmentRepository = assignmentRepository;
			_storeOrderRepository = storeOrderRepository;
			_userWarehouseRepository = userWarehouseRepository;
			_repositoryShelfOrder = repositoryShelfOrder;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// ================= CREATE =================
		public async Task<ShipmentAssignmentResponse> CreateAsync(
			CreateShipmentAssignmentRequest request,
			ICurrentUser currentUser)
		{
			if (request.StoreOrderId == null && request.ShelfOrderId == null)
				throw new AppException("Order is required", 400);

			if (request.StoreOrderId != null && request.ShelfOrderId != null)
				throw new AppException("Only one order type is allowed", 400);

			// ================= STORE ORDER =================
			if (request.StoreOrderId != null)
			{
				var order = await _storeOrderRepository.GetByIdAsync(request.StoreOrderId.Value);

				if (order == null)
					throw new AppException("Store order not found", 404);

				if (order.Status != StoreOrderStatus.Approved)
					throw new AppException("Store order must be approved", 400);
			}

			// ================= SHELF ORDER =================
			if (request.ShelfOrderId != null)
			{
				var order = await _repositoryShelfOrder.GetByIdAsync(request.ShelfOrderId.Value);

				if (order == null)
					throw new AppException("Shelf order not found", 404);

				if (order.Status != ShelfOrderStatus.Approved)
					throw new AppException("Shelf order must be approved", 400);
			}

			var assignment = new ShipmentAssignment
			{
				Id = Guid.NewGuid(),
				StoreOrderId = request.StoreOrderId,
				ShelfOrderId = request.ShelfOrderId,
				WarehouseLocationId = request.WarehouseLocationId,
				Status = AssignmentStatus.Pending,
				CreatedAt = _dateTime.UtcNow,
				CreatedByUserId = currentUser.UserId
			};

			await _assignmentRepository.AddAsync(assignment);
			await _unitOfWork.SaveChangesAsync();

			var result = await _assignmentRepository.GetByIdWithDetailsAsync(assignment.Id);

			if (result == null)
				throw new AppException("Shipment assignment not found", 404);

			return MapToResponse(result);
		}


		// ================= ASSIGN SHIPPER (WAREHOUSE) =================
		public async Task AssignShipperAsync(AssignShipperRequest request, ICurrentUser currentUser)
		{
			// 1. Lấy assignment 
			var assignment = await _assignmentRepository
				.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 2. Lấy WarehouseId thật (QUAN TRỌNG)
			var warehouseId = assignment.WarehouseLocation?.WarehouseId
				?? throw new AppException("Warehouse not found", 500);

			// 3. Check Manager 
			var userWarehouse = await _userWarehouseRepository
				.GetActiveAsync(currentUser.UserId, warehouseId);

			if (userWarehouse == null || userWarehouse.Role != WarehouseRole.Manager)
				throw new AppException("Only Manager can assign shipper", 403);

			//  4. Check shipper hợp lệ 
			var shipperWarehouse = await _userWarehouseRepository
				.GetActiveAsync(request.ShipperId, warehouseId);

			if (shipperWarehouse == null || shipperWarehouse.Role != WarehouseRole.Shipper)
				throw new AppException("User is not a shipper in this warehouse", 400);

			// 5. Check shipment đang chạy 
			var activeShipment = assignment.Shipments
				.FirstOrDefault(s => s.Status == ShipmentStatus.Draft
								  || s.Status == ShipmentStatus.Shipping);

			if (activeShipment != null)
				throw new AppException("Cannot change Shipper, shipment is in progress", 400);

			//  6. Assign 
			var oldShipperId = assignment.ShipperId;

			assignment.ShipperId = request.ShipperId;
			assignment.AssignedByUserId = currentUser.UserId;

			//  7. Update status 
			if (oldShipperId != request.ShipperId || assignment.Status == AssignmentStatus.Rejected)
			{
				assignment.Status = AssignmentStatus.Assigned;
				assignment.RespondedAt = null;
			}

			_assignmentRepository.Update(assignment);
			await _unitOfWork.SaveChangesAsync();
		}



		public async Task AcceptAsync(Guid id, ICurrentUser currentUser)
		{
			// 1. Lấy assignment 
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 2. Lấy WarehouseId chuẩn 
			var warehouseId = assignment.WarehouseLocation?.WarehouseId
				?? throw new AppException("Warehouse not found", 500);

			// 3. Check role Shipper 
			var userWarehouse = await _userWarehouseRepository
				.GetActiveAsync(currentUser.UserId, warehouseId);

			if (userWarehouse == null || userWarehouse.Role != WarehouseRole.Shipper)
				throw new AppException("Only Shipper can accept assignment", 403);

			// 4. Check đúng người được assign 
			if (assignment.ShipperId != currentUser.UserId)
				throw new AppException("You are not assigned to this shipment", 403);

			// 5. Check trạng thái 
			if (assignment.Status != AssignmentStatus.Assigned)
				throw new AppException("Assignment already responded", 400);

			// 6. Update 
			assignment.Status = AssignmentStatus.Accepted;
			assignment.RespondedAt = _dateTime.UtcNow;

			_assignmentRepository.Update(assignment);
			await _unitOfWork.SaveChangesAsync();
		}


		public async Task RejectAsync(Guid id, ICurrentUser currentUser)
		{
			// 1. Lấy assignment 
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			// 2. Lấy WarehouseId 
			var warehouseId = assignment.WarehouseLocation?.WarehouseId
				?? throw new AppException("Warehouse not found", 500);

			// 3. Check role Shipper 
			var userWarehouse = await _userWarehouseRepository
				.GetActiveAsync(currentUser.UserId, warehouseId);

			if (userWarehouse == null || userWarehouse.Role != WarehouseRole.Shipper)
				throw new AppException("Only Shipper can reject assignment", 403);

			// 4. Check đúng người
			if (assignment.ShipperId != currentUser.UserId)
				throw new AppException("You are not assigned to this shipment", 403);

			// 5. Check trạng thái
			if (assignment.Status != AssignmentStatus.Assigned)
				throw new AppException("Assignment already responded", 400);

			// 6. Update
			assignment.Status = AssignmentStatus.Rejected;
			assignment.RespondedAt = _dateTime.UtcNow;

			_assignmentRepository.Update(assignment);
			await _unitOfWork.SaveChangesAsync();
		}


		// ================= GET MY ASSIGNMENTS =================
		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetMyAssignments(ICurrentUser currentUser)
		{
			var assignments = await _assignmentRepository.GetByShipperIdWithOrderAsync(currentUser.UserId);

			return assignments.Select(MapToResponse);
		}

		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetByStoreOrderId(Guid storeOrderId)
		{
			var assignments = await _assignmentRepository
				.GetByStoreOrderIdWithDetailsAsync(storeOrderId);

			return assignments.Select(MapToResponse);
		}

		public async Task<IEnumerable<ShipmentAssignmentResponse>> GetAllAsync()
		{
			var assignments = await _assignmentRepository.GetAllWithDetailsAsync();

			return assignments.Select(MapToResponse);
		}

		public async Task<ShipmentAssignmentResponse> GetByIdAsync(Guid id)
		{
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			return MapToResponse(assignment);
		}


		private static ShipmentAssignmentResponse MapToResponse(ShipmentAssignment assignment)
		{
			var isStoreOrder = assignment.StoreOrderId != null;

			var shipment = assignment.Shipments?
				.OrderByDescending(s => s.CreatedAt)
				.FirstOrDefault();

			var response = new ShipmentAssignmentResponse
			{
				Id = assignment.Id,

				// ===== ORDER INFO =====
				StoreOrderId = assignment.StoreOrderId,
				StoreOrderCode = isStoreOrder ? assignment.StoreOrder?.Code : null,

				ShelfOrderId = assignment.ShelfOrderId,
				ShelfOrderCode = !isStoreOrder ? assignment.ShelfOrder?.Code : null,

				OrderType = isStoreOrder ? "STORE" : "SHELF",

				// ===== LOCATION =====
				WarehouseLocationId = assignment.WarehouseLocationId,
				WarehouseLocationName = assignment.WarehouseLocation?.Name ?? "",

				StoreLocationId = isStoreOrder
					? assignment.StoreOrder?.StoreLocationId ?? Guid.Empty
					: assignment.ShelfOrder?.StoreLocationId ?? Guid.Empty,

				StoreLocationName = isStoreOrder
					? assignment.StoreOrder?.StoreLocation?.Name ?? ""
					: assignment.ShelfOrder?.StoreLocation?.Name ?? "",

				// ===== USER =====
				ShipperName = assignment.Shipper?.FullName,

				CreatedByName = assignment.CreatedByUser?.FullName
					?? throw new Exception("CreatedByUser not loaded"),

				AssignedByName = assignment.AssignedByUser?.FullName,

				// ===== STATUS =====
				Status = assignment.Status,
				ShipmentStatus = shipment?.Status ?? ShipmentStatus.Draft,

				CreatedAt = assignment.CreatedAt,
				RespondedAt = assignment.RespondedAt
			};

			// ================= STORE ORDER =================
			if (isStoreOrder)
			{
				response.ProductItems = assignment.StoreOrder?.Items
					.Select(x => new ShipmentAssignmentProductItemResponse
					{
						ProductColorId = x.ProductColorId,
						SKU = x.ProductColor?.Product?.SKU!,
						ProductName = x.ProductColor?.Product?.Name!,
						Color = x.ProductColor?.Color?.Name!,
						ImageUrl = x.ProductColor?.ImageUrl,
						Quantity = x.Quantity,
						FulfilledQuantity = x.FulfilledQuantity
					}).ToList();
			}
			// ================= SHELF ORDER =================
			else
			{
				response.ShelfItems = assignment.ShelfOrder?.Items
					.Select(x => new ShipmentAssignmentShelfItemResponse
					{
						ShelfTypeId = x.ShelfTypeId,
						ShelfTypeName = x.ShelfTypeName,
						ImageUrl = x.ImageUrl,
						Quantity = x.Quantity,
						FulfilledQuantity = x.FulfilledQuantity
					}).ToList();
			}

			return response;
		}
	}
}
