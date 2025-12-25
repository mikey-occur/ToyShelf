using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;
using ToyCabin.Infrastructure.Context;

namespace ToyCabin.Infrastructure.Repositories
{
	public class AccountRepository : GenericRepository<Account>, IAccountRepository
	{
		public AccountRepository(ToyCabinDbContext context) : base(context){}
		public async Task<Account?> GetLocalAccountByEmailAsync(string email)
		{
			return await _context.Accounts
				.Include(a => a.User)
				.Include(a => a.AccountRoles)
					.ThenInclude(ar => ar.Role)
				.FirstOrDefaultAsync(a =>
					a.Provider == AuthProvider.LOCAL &&
					a.User.Email == email);
		}

		public async Task<bool> ExistsLocalAccountByEmailAsync(string email)
		{
			return await _context.Accounts
				.AnyAsync(a =>
					a.Provider == AuthProvider.LOCAL &&
					a.User.Email == email);
		}

		public async Task<Account?> GetAccountByEmailAndProviderAsync(string email, AuthProvider provider)
		{
			return await _context.Accounts
				.Include(a => a.User)
				.Include(a => a.AccountRoles)
					.ThenInclude(ar => ar.Role)
				.FirstOrDefaultAsync(a => a.User.Email == email && a.Provider == provider);
		}

		public async Task<Account?> GetByIdWithUserAsync(Guid id)
		{
			return await _context.Accounts
				.Include(a => a.User)
				.Include(a => a.AccountRoles)
					.ThenInclude(ar => ar.Role)
				.FirstOrDefaultAsync(a => a.Id == id);
		}
	}
}
