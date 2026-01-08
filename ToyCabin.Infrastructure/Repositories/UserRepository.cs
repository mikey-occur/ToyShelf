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
	public class UserRepository : GenericRepository<User>, IUserRepository
	{
		public UserRepository(ToyCabinDbContext context) : base(context){}
		public async Task<List<User>> GetActiveUsersAsync()
		{
			return await _context.Users
				.Where(u => u.IsActive)
				.ToListAsync();
		}

		public async Task<List<User>> GetInactiveUsersAsync()
		{
			return await _context.Users
				.Where(u => !u.IsActive)
				.ToListAsync();
		}
		public async Task<User?> GetByEmailAsync(string email)
		{
			return await _context.Users
				.FirstOrDefaultAsync(x =>
					x.Email == email &&
					x.IsActive);
		}
	}
}
