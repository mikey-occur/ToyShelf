using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ShipmentService : IShipmentService
	{
		private readonly IShipmentRepository _shipmentRepository;
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;


		public ShipmentService(
			IShipmentRepository shipmentRepository,
			IStoreOrderRepository storeOrderRepository,
			IUnitOfWork unitOfWork,
			IDateTimeProvider dateTime)
		{
			_shipmentRepository = shipmentRepository;
			_storeOrderRepository = storeOrderRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		//public async Task<Guid> CreateAsync(CreateShipmentRequest request)
		//{
		//	var order = await _storeOrderRepository.GetByIdAsync(request.StoreOrderId);

		//	if (order == null)
		//		throw new AppException("Store order not found", 404);

		//	if (order.Status != StoreOrderStatus.Approved)
		//		throw new AppException("Order must be approved", 400);

		//	var shipment = new Shipment
		//	{
		//		Id = Guid.NewGuid(),
		//		Code = $"SH-{Guid.NewGuid().ToString().Substring(0, 6)}",
		//		StoreOrderId = request.StoreOrderId,
		//		FromLocationId = request.FromLocationId,
		//		ToLocationId = request.ToLocationId,
		//		RequestedByUserId = request.RequestedByUserId,
		//		ShipperId = request.ShipperId,
		//		Status = ShipmentStatus.Draft,
		//		CreatedAt = _dateTime.UtcNow
		//	};

		//	foreach (var item in request.Items)
		//	{
		//		shipment.Items.Add(new ShipmentItem
		//		{
		//			Id = Guid.NewGuid(),
		//			ProductColorId = item.ProductColorId,
		//			ExpectedQuantity = item.Quantity
		//		});
		//	}

		//	await _shipmentRepository.AddAsync(shipment);

		//	await _unitOfWork.SaveChangesAsync();

		//	return shipment.Id;
		//}



	}
}
