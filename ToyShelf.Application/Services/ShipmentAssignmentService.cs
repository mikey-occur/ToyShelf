using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
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
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public ShipmentAssignmentService(
			IShipmentAssignmentRepository assignmentRepository,
			IStoreOrderRepository storeOrderRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_assignmentRepository = assignmentRepository;
			_storeOrderRepository = storeOrderRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// ================= CREATE =================
		public async Task<ShipmentAssignmentResponse> CreateAsync(
			CreateShipmentAssignmentRequest request,
			ICurrentUser currentUser)
		{
			var order = await _storeOrderRepository.GetByIdAsync(request.StoreOrderId);

			if (order == null)
				throw new AppException("Store order not found", 404);

			if (order.Status != StoreOrderStatus.Approved)
				throw new AppException("Store order must be approved before assigning shipper", 400);

			var assignment = new ShipmentAssignment
			{
				Id = Guid.NewGuid(),
				WarehouseLocationId = request.WarehouseLocationId,
				StoreOrderId = request.StoreOrderId,
				ShipperId = request.ShipperId,
				Status = AssignmentStatus.Pending,
				CreatedAt = _dateTime.UtcNow,
				AssignedByUserId = currentUser.UserId
			};

			await _assignmentRepository.AddAsync(assignment);
			await _unitOfWork.SaveChangesAsync();

			var result = await _assignmentRepository.GetByIdWithDetailsAsync(assignment.Id);

			if (result == null)
				throw new AppException("Shipment assignment not found", 404);

			return MapToResponse(result);
		}

		// ================= SHIPPER ACCEPT =================
		public async Task AcceptAsync(Guid id, ICurrentUser currentUser)
		{
			var assignment = await _assignmentRepository.GetByIdAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			if (assignment.ShipperId != currentUser.UserId)
				throw new AppException("You are not assigned to this shipment", 403);

			if (assignment.Status != AssignmentStatus.Pending)
				throw new AppException("Assignment already responded", 400);

			assignment.Status = AssignmentStatus.Accepted;
			assignment.RespondedAt = _dateTime.UtcNow;

			_assignmentRepository.Update(assignment);

			await _unitOfWork.SaveChangesAsync();
		}

		// ================= SHIPPER REJECT =================
		public async Task RejectAsync(Guid id, ICurrentUser currentUser)
		{
			var assignment = await _assignmentRepository.GetByIdAsync(id);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			if (assignment.ShipperId != currentUser.UserId)
				throw new AppException("You are not assigned to this shipment", 403);

			if (assignment.Status != AssignmentStatus.Pending)
				throw new AppException("Assignment already responded", 400);

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

		private static ShipmentAssignmentResponse MapToResponse(ShipmentAssignment assignment)
		{
			return new ShipmentAssignmentResponse
			{
				Id = assignment.Id,

				StoreOrderId = assignment.StoreOrderId,

				StoreOrderCode = assignment.StoreOrder.Code,

				WarehouseLocationId = assignment.WarehouseLocationId,

				WarehouseLocationName = assignment.WarehouseLocation.Name,

				StoreLocationId = assignment.StoreOrder.StoreLocationId,

				StoreLocationName = assignment.StoreOrder.StoreLocation.Name,

				ShipperName = assignment.Shipper.FullName,

				AssignedByName = assignment.AssignedByUser.FullName,

				Status = assignment.Status,

				CreatedAt = assignment.CreatedAt,

				RespondedAt = assignment.RespondedAt,

				Items = assignment.StoreOrder.Items.Select(x => new ShipmentAssignmentItemResponse
				{
					ProductName = x.ProductColor.Product.Name,
					Color = x.ProductColor.Color.Name,
					Quantity = x.Quantity
				}).ToList()
			};
		}
	}

}
