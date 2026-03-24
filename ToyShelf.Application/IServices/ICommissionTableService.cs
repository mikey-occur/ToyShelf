using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.PriceTable.Request;
using ToyShelf.Application.Models.PriceTable.Response;
using ToyShelf.Application.Models.ProductColor.Response;

namespace ToyShelf.Application.IServices
{
	public interface ICommissionTableService
	{
		Task<CommissionTableResponse> CreateAsync(CommissionTableRequest request);
		Task<CommissionTableResponse> GetByIdAsync(Guid id);
		Task<IEnumerable<CommissionTableResponse>> GetPriceTablesAsync(bool? isActive);
		Task<IEnumerable<CommissionTableResponse>> GetAllAsync();
		Task<bool> RestorePriceTableAsync(Guid id);
		Task<bool> DisablePriceTableAsync(Guid id);
		Task<CommissionTableResponse> UpdateAsync(Guid id, CommissionTableUpdateRequest request);
		Task<bool> DeleteAsync(Guid id);
	}
}
