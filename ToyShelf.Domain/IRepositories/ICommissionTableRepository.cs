using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface ICommissionTableRepository : IGenericRepository<CommissionTable>
	{
		Task<CommissionTable?> GetByIdWithDetailsAsync(Guid id);

		Task<bool> IsPriceTableInUseAsync(Guid id);

		Task<IEnumerable<CommissionTable>> GetPriceTablesAsync(bool? isActive);
	}
}
