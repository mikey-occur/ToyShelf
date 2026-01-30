using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IUserRepository : IGenericRepository<User>
	{
		Task<List<User>> GetUsersAsync(bool? isActive);
		Task<User?> GetByEmailAsync(string email);
	}
}
