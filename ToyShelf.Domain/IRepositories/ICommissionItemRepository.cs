using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface ICommissionItemRepository : IGenericRepository<CommissionItem>
	{
		//Task<CommissionItem?> GetItemAsync(Guid priceTableId, Guid segmentId);
	}
}
