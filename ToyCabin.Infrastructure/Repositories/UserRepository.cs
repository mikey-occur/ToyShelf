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
		public async Task<List<User>> GetUsersAsync(bool? isActive)
		{
			var query = _context.Users.AsQueryable();

			if (isActive.HasValue)
			{
				query = query.Where(u => u.IsActive == isActive.Value);
			}

			return await query.ToListAsync();
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
