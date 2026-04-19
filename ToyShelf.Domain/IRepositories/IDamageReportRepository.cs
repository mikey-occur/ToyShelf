using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IDamageReportRepository : IGenericRepository<DamageReport>
	{
		Task<int> GetMaxSequenceAsync();
		Task<IEnumerable<DamageReport>> GetAllWithIncludeAsync(DamageStatus? status, Guid? partnerId = null, Guid? storeId = null);
		Task<DamageReport?> GetByIdFullIncludeAsync(Guid id);
	}
}
