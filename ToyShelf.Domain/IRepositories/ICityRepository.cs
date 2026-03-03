using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface ICityRepository : IGenericRepository<City>
	{
		Task<bool> ExistsByCodeOrNameAsync(string code, string name);
		Task<bool> IsDuplicateAsync(Guid id, string code, string name);
	}
}
