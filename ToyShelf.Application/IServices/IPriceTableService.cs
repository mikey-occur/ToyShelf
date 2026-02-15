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
	public interface IPriceTableService
	{
		Task<PriceTableResponse> CreateAsync(PriceTableRequest request);
		Task<PriceTableResponse> GetByIdAsync(Guid id);
		Task<IEnumerable<PriceTableResponse>> GetPriceTablesAsync(bool? isActive);
		Task<IEnumerable<PriceTableResponse>> GetAllAsync();
		Task<bool> RestorePriceTableAsync(Guid id);
		Task<bool> DisablePriceTableAsync(Guid id);
		Task<PriceTableResponse> UpdateAsync(Guid id, PriceTableUpdateRequest request);
		Task<bool> DeleteAsync(Guid id);
	}
}
