using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Domain.IRepositories
{
	public interface IAccountRepository : IGenericRepository<Account>
	{
		Task<Account?> GetLocalAccountByEmailAsync(string email);
		Task<bool> ExistsLocalAccountByEmailAsync(string email);
	}
}
