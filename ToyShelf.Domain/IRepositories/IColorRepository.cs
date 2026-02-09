using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IColorRepository : IGenericRepository<Color>
	{
		Task<bool> ExistsByNameOrHexAsync(string name, string hexCode);
		Task<bool> IsDuplicateAsync(Guid id, string name, string hexCode);
	}
}
