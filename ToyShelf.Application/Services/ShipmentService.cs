using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.Shipment.Response;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ShipmentService : IShipmentService
	{
		private readonly IShipmentRepository _shipmentRepository;
		private readonly IShipmentAssignmentRepository _assignmentRepository;
		private readonly IShipmentItemRepository _shipmentItemRepository;
		private readonly IShipmentMediaRepository _shipmentMediaRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		private const string Prefix = "SH";

		public ShipmentService(
			IShipmentRepository shipmentRepository,
			IShipmentAssignmentRepository assignmentRepository,
			IShipmentItemRepository shipmentItemRepository,
			IShipmentMediaRepository shipmentMediaRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_shipmentRepository = shipmentRepository;
			_assignmentRepository = assignmentRepository;
			_shipmentItemRepository = shipmentItemRepository;
			_shipmentMediaRepository = shipmentMediaRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		public async Task<IEnumerable<ShipmentResponse>> GetAllAsync(ShipmentStatus? shipmentStatus)
		{
			var shipments = await _shipmentRepository.GetAllWithDetailsAsync(shipmentStatus);

			return shipments.Select(MapToResponse);
		}
		public async Task<ShipmentResponse> GetByAssignmentIdAsync(Guid assignmentId)
		{
			var shipment = await _shipmentRepository.GetByAssignmentIdAsync(assignmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			return MapToResponse(shipment);
		}

		public async Task<ShipmentResponse> GetByIdAsync(Guid shipmentId)
		{
			var shipment = await _shipmentRepository.GetByIdWithDetailsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			return MapToResponse(shipment);
		}

		public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, ICurrentUser currentUser)
		{
			var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.ShipmentAssignmentId);

			if (assignment == null)
				throw new AppException("Assignment not found", 404);

			if (assignment.Status == AssignmentStatus.Rejected)
				throw new AppException("Shipper has rejected this assignment", 400);

			if (assignment.Status != AssignmentStatus.Accepted)
				throw new AppException("Shipper must accept assignment first", 400);

			if (assignment.Shipment != null)
				throw new AppException("Shipment already created", 400);

			var code = await GenerateCode();

			var shipment = new Shipment
			{
				Id = Guid.NewGuid(),
				Code = code,
				StoreOrderId = assignment.StoreOrderId,
				ShipmentAssignmentId = assignment.Id,
				FromLocationId = assignment.WarehouseLocationId,
				ToLocationId = assignment.StoreOrder.StoreLocationId,
				RequestedByUserId = currentUser.UserId,
				Status = ShipmentStatus.Draft,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentRepository.AddAsync(shipment);

			foreach (var item in assignment.StoreOrder.Items)
			{
				var shipmentItem = new ShipmentItem
				{
					Id = Guid.NewGuid(),
					ShipmentId = shipment.Id,
					ProductColorId = item.ProductColorId,
					ExpectedQuantity = item.Quantity,
					ReceivedQuantity = 0
				};

				await _shipmentItemRepository.AddAsync(shipmentItem);
			}

			await _unitOfWork.SaveChangesAsync();

			var result = await _shipmentRepository.GetByIdWithDetailsAsync(shipment.Id);

			if (result == null)
				throw new AppException("Shipment not found", 404);

			return MapToResponse(result);
		}

		public async Task PickupAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Draft)
				throw new AppException("Shipment not ready for pickup", 400);


			var media = new ShipmentMedia
			{
				Id = Guid.NewGuid(),
				ShipmentId = shipmentId,
				UploadedByUserId = currentUser.UserId,
				MediaUrl = request.MediaUrl,
				MediaType = ShipmentMediaType.Image,
				Purpose = ShipmentMediaPurpose.Pickup,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentMediaRepository.AddAsync(media);

			shipment.Status = ShipmentStatus.Shipping;
			shipment.PickedUpAt = _dateTime.UtcNow;

			_shipmentRepository.Update(shipment);

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task DeliveryAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser)
		{
			var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Shipping)
				throw new AppException("Shipment not shipping", 400);

			var media = new ShipmentMedia
			{
				Id = Guid.NewGuid(),
				ShipmentId = shipmentId,
				UploadedByUserId = currentUser.UserId,
				MediaUrl = request.MediaUrl,
				MediaType = ShipmentMediaType.Image,
				Purpose = ShipmentMediaPurpose.Delivery,
				CreatedAt = _dateTime.UtcNow
			};

			await _shipmentMediaRepository.AddAsync(media);

			shipment.DeliveredAt = _dateTime.UtcNow;

			_shipmentRepository.Update(shipment);

			await _unitOfWork.SaveChangesAsync();
		}

		public async Task ReceiveAsync(Guid shipmentId, ReceiveShipmentRequest request)
		{
			var shipment = await _shipmentRepository.GetByIdWithItemsAsync(shipmentId);

			if (shipment == null)
				throw new AppException("Shipment not found", 404);

			if (shipment.Status != ShipmentStatus.Shipping)
				throw new AppException("Shipment not shipping", 400);

			foreach (var item in shipment.Items)
			{
				var reqItem = request.Items
					.FirstOrDefault(x => x.ProductColorId == item.ProductColorId);

				if (reqItem == null)
					throw new AppException("Missing item data", 400);

				item.ReceivedQuantity = reqItem.ReceivedQuantity;
			}

			shipment.Status = ShipmentStatus.Received;
			shipment.ReceivedAt = _dateTime.UtcNow;

			await _unitOfWork.SaveChangesAsync();
		}

		private async Task<string> GenerateCode()
		{
			var max = await _shipmentRepository.GetMaxSequenceAsync();

			return $"{Prefix}-{(max + 1):D5}";
		}

		private static ShipmentResponse MapToResponse(Shipment shipment)
		{
			return new ShipmentResponse
			{
				Id = shipment.Id,
				Code = shipment.Code,
				StoreOrderId = shipment.StoreOrderId,
				FromLocationId = shipment.FromLocationId,
				FromLocationName = shipment.FromLocation.Name,
				ToLocationId = shipment.ToLocationId,
				ToLocationName = shipment.ToLocation.Name,
				ShipperName = shipment.ShipmentAssignment.Shipper?.FullName,
				Status = shipment.Status,
				CreatedAt = shipment.CreatedAt,
				PickedUpAt = shipment.PickedUpAt,
				DeliveredAt = shipment.DeliveredAt,
				ReceivedAt = shipment.ReceivedAt,
				Items = shipment.Items.Select(x => new ShipmentItemResponse
				{
					ProductName = x.ProductColor.Product.Name,
					Color = x.ProductColor.Color.Name,
					ExpectedQuantity = x.ExpectedQuantity,
					ReceivedQuantity = x.ReceivedQuantity
				}).ToList()
			};
		}
	}
}
