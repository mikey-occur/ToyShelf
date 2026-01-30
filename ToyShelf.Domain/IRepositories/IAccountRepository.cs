using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IAccountRepository : IGenericRepository<Account>
	{
		Task<Account?> GetLocalAccountByEmailAsync(string email);
		Task<bool> ExistsLocalAccountByEmailAsync(string email);
		Task<Account?> GetAccountByEmailAndProviderAsync(string email, AuthProvider provider);
		Task<Account?> GetByIdWithUserAsync(Guid id);
	}
}
