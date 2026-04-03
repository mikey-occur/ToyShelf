using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IShelfTransactionRepository : IGenericRepository<ShelfTransaction>
	{
		Task<List<Shelf>> GetShelvesByShipmentAndType(Guid shipmentId, Guid shelfTypeId);
		Task<List<Shelf>> GetShelvesByShipment(Guid shipmentId);
	}
}
