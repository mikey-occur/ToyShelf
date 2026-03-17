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
		Task<Shipment?> GetByAssignmentIdAsync(Guid assignmentId);
		Task<IEnumerable<Shipment>> GetAllWithDetailsAsync(ShipmentStatus? shipmentStatus);
	}
}
