using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IShipmentRepository : IGenericRepository<Shipment>
	{
		Task<int> GetMaxSequenceAsync();
		Task<Shipment?> GetByIdWithDetailsAsync(Guid id);
		Task<List<Shipment>> GetListByAssignmentIdAsync(Guid assignmentId);
		Task<IEnumerable<Shipment>> GetAllWithDetailsAsync(ShipmentStatus? shipmentStatus);
		Task<Shipment?> GetByIdWithItemsAsync(Guid id);
		Task<List<Shipment>> GetByStoreOrderIdAsync(Guid storeOrderId);
		Task<List<Shipment>> GetByShelfOrderIdAsync(Guid shelfOrderId);
		Task<Shipment?> GetByIdWithShelfItemsAsync(Guid id);
		Task<(int TotalDelivering, int TotalCompleted, int TotalCancelled, int TotalAll)> GetShipperStatsAsync(Guid shipperId);
	}
}
