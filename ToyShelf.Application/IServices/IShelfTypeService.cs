using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.ShelfType.Request;
using ToyShelf.Application.Models.ShelfType.Response;

namespace ToyShelf.Application.IServices
{
	public interface IShelfTypeService
	{
		Task<ShelfTypeResponse> CreateAsync(ShelfTypeRequest request);
		Task<IEnumerable<ShelfTypeResponse>> GetShelfTypesAsync(bool? isActive, string? searchName = null, string? categoryType = null);
		Task<ShelfTypeResponse> GetByIdAsync(Guid id);
		Task<ShelfTypeResponse> UpdateAsync(Guid id, ShelfTypeRequest request);
		Task DeleteAsync(Guid id);
		Task<bool> DisableAsync(Guid id);
		Task<bool> RestoreAsync(Guid id);


	}
}
