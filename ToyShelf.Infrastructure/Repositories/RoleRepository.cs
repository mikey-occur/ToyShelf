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
	public class RoleRepository : GenericRepository<Role>, IRoleRepository
	{
		public RoleRepository(ToyCabinDbContext context) : base(context) {}

		public async Task<List<Role>> GetRolesByUserIdAsync(Guid userId)
		{
			return await _context.AccountRoles
				.Where(ar =>
					ar.Account.UserId == userId &&
					ar.Role.IsActive)
				.Select(ar => ar.Role)
				.Distinct()
				.ToListAsync();
		}
		public async Task<Role?> GetByNameAsync(string name)
		{
			return await _context.Roles
				.AsNoTracking()
				.FirstOrDefaultAsync(r =>
					r.Name == name &&
					r.IsActive);
		}
		public async Task<IEnumerable<Role>> GetRolesAsync(bool? isActive)
		{
			var query = _context.Roles.AsQueryable();

			if (isActive.HasValue)
				query = query.Where(s => s.IsActive == isActive.Value);

			return await query
				.OrderByDescending(w => w.CreatedAt)
				.ToListAsync();
		}
	}
}
