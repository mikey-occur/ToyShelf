using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IPriceTableRepository : IGenericRepository<PriceTable>
	{
		Task<PriceTable?> GetByIdWithDetailsAsync(Guid id);

		Task<bool> IsPriceTableInUseAsync(Guid id);

		Task<IEnumerable<PriceTable>> GetPriceTablesAsync(bool? isActive);
	}
}
