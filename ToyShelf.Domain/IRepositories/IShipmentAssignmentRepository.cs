using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IShipmentAssignmentRepository : IGenericRepository<ShipmentAssignment>
	{
		Task<IEnumerable<ShipmentAssignment>> GetByShipperIdWithOrderAsync(Guid shipperId);
		Task<ShipmentAssignment?> GetByIdWithDetailsAsync(Guid id);
		Task<IEnumerable<ShipmentAssignment>> GetByStoreOrderIdWithDetailsAsync(Guid storeOrderId);
		Task<IEnumerable<ShipmentAssignment>> GetByShelfOrderIdWithDetailsAsync(Guid shelfOrderId);
		Task<IEnumerable<ShipmentAssignment>> GetByDamageReportIdWithDetailsAsync(Guid damageReportId);
		Task<IEnumerable<ShipmentAssignment>> GetAllWithDetailsAsync(
				AssignmentType? type,
				AssignmentStatus? status);
		Task<ShipmentAssignment?> GetPendingByLocationAsync(Guid warehouseLocationId, Guid storeLocationId);
		Task<int> GetTotalAllocatedQuantityAsync(Guid storeOrderId, Guid storeOrderItemId);
		Task<int> GetTotalShelfAllocatedQuantityAsync(Guid shelfOrderId, Guid shelfOrderItemId);
		Task<Dictionary<(Guid locationId, Guid productColorId), int>> GetAllocatedQuantitiesAsync(List<Guid> locationIds, List<Guid> productColorIds);
		Task<Dictionary<(Guid locationId, Guid shelfTypeId), int>> GetAllocatedShelfQuantitiesAsync(
			List<Guid> locationIds,
			List<Guid> shelfTypeIds);
	}
}
