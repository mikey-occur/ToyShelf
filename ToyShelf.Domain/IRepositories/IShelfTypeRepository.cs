using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IShelfTypeRepository : IGenericRepository<ShelfType>
	{
		Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
		Task<IEnumerable<ShelfType>> GetShelfTypesAsync(bool? isActive, string? searchName = null, string? categoryType = null);

		Task<ShelfType?> GetByIdWithLevelsAsync(Guid id);
	}
}
