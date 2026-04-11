using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.Shipment.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IShipmentService
	{
		Task<IEnumerable<ShipmentResponse>> GetAllAsync(ShipmentStatus? shipmentStatus);
		Task<IEnumerable<ShipmentResponse>> GetByAssignmentIdAsync(Guid assignmentId);
		Task<ShipmentResponse> GetByIdAsync(Guid shipmentId);
		Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, ICurrentUser currentUser);
		Task PickupAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser);
		Task PickupReturnAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser);
		Task DeliveryAsync(Guid shipmentId, UploadShipmentMediaRequest request, ICurrentUser currentUser);
		Task ArrivedWarehouseAsync(Guid shipmentId, ICurrentUser currentUser);
		Task StoreReceiveAsync(Guid shipmentId, StoreReceiveRequest request);
		Task WarehouseReceiveReturnAsync(Guid shipmentId);
		Task<IEnumerable<ShipmentResponse>> GetByStoreOrderIdAsync(Guid storeOrderId);
		Task<IEnumerable<ShelfSimpleResponse>> GetShelvesByShipmentAsync(Guid shipmentId);
		Task<List<ShelfShipmentItemResponse>> GetShelfItemsAsync(Guid shipmentId);

		// Hỗ trợ StoreReceiveAsync
		Task<ShipmentReceiveViewModel> GetShipmentForReceivingAsync(Guid shipmentId);
	}
}
