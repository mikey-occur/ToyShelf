using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
	public class UserRepository : GenericRepository<User>, IUserRepository
	{
		public UserRepository(ToyShelfDbContext context) : base(context){}
		public async Task<List<User>> GetUsersAsync(bool? isActive)
		{
			var query = _context.Users.AsQueryable();

			if (isActive.HasValue)
			{
				query = query.Where(u => u.IsActive == isActive.Value);
			}

			return await query
					.OrderByDescending(w => w.CreatedAt)
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
