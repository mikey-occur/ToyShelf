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
	}
}
