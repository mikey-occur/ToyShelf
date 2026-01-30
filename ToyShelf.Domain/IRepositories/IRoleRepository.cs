using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IRoleRepository : IGenericRepository<Role> 
	{
		Task<List<Role>> GetRolesByUserIdAsync(Guid userId);
		Task<Role?> GetByNameAsync(string name);
		Task<IEnumerable<Role>> GetRolesAsync(bool? isActive);
	}
}
