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
		public async Task<List<User>> GetUsersAsync(
			bool? isActive,
			string? role)
		{
			var query = _context.Users
				.Include(u => u.Accounts)
					.ThenInclude(a => a.AccountRoles)
						.ThenInclude(ar => ar.Role)
				.Include(u => u.UserStores)
				.Include(u => u.UserWarehouses)
				.AsQueryable();

			if (isActive.HasValue)
			{
				query = query.Where(u => u.IsActive == isActive.Value);
			}

			if (!string.IsNullOrEmpty(role))
			{
				query = query.Where(u =>
					u.Accounts.Any(a =>
						a.AccountRoles.Any(ar =>
							ar.Role.Name == role)
					)
				);
			}

			return await query.ToListAsync();
		}

		public async Task<List<User>> GetUsersByStoreOrPartnerAsync()
		{
			return await _context.Users
					.Include(u => u.UserStores)
						.ThenInclude(us => us.Store)
					.OrderByDescending(u => u.CreatedAt)
					.ToListAsync();
		}

		public async Task<User?> GetUserWithPartnerAsync(Guid userId)
		{
			return await _context.Users
				.Include(x => x.Partner)
				.FirstOrDefaultAsync(x => x.Id == userId);
		}

		public async Task<User?> GetUserWithWarehousesAsync(Guid userId)
		{
			return await _context.Users
				.Include(u => u.UserWarehouses)
					.ThenInclude(uw => uw.Warehouse)
						.ThenInclude(w => w.InventoryLocations)
				.FirstOrDefaultAsync(u => u.Id == userId);
		}


		public async Task<User?> GetByEmailAsync(string email)
		{
			return await _context.Users
				.FirstOrDefaultAsync(x =>
					x.Email == email &&
					x.IsActive);
		}
		public async Task<User?> GetPartnerAdminByPartnerIdAsync(Guid partnerId)
		{
			return await _context.Users
				.Include(u => u.Accounts)
					.ThenInclude(a => a.AccountRoles)
						.ThenInclude(ar => ar.Role)
				.Where(u => u.PartnerId == partnerId)
				.FirstOrDefaultAsync(u =>
					u.Accounts.Any(a =>
						a.AccountRoles.Any(ar =>
							ar.Role.Name == "PartnerAdmin")));
		}
		public async Task<User?> GetUserWithRolesAsync(Guid userId)
		{
			return await _context.Users
				.Include(u => u.Accounts)
					.ThenInclude(a => a.AccountRoles)
						.ThenInclude(ar => ar.Role)
				.FirstOrDefaultAsync(u => u.Id == userId);
		}
		public async Task<List<User>> GetUsersWithWarehousesAsync()
		{
			return await _context.Users
				.Include(u => u.UserWarehouses)
					.ThenInclude(uw => uw.Warehouse)
						.ThenInclude(w => w.InventoryLocations)
				.ToListAsync();
		}
	}
}
