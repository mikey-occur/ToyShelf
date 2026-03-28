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
	public class UserWarehouseRepository : GenericRepository<UserWarehouse>, IUserWarehouseRepository
	{
		public UserWarehouseRepository(ToyShelfDbContext context) : base(context){}
		public async Task<UserWarehouse?> GetActiveAsync(Guid userId, Guid warehouseId)
		{
			return await _context.UserWarehouses
				.FirstOrDefaultAsync(x =>
					x.UserId == userId &&
					x.WarehouseId == warehouseId &&
					x.IsActive);
		}
		public async Task<List<UserWarehouse>> GetUsersByWarehouseIdAsync(
			Guid warehouseId,
			WarehouseRole? role)
		{
			var query = _context.UserWarehouses
				.Include(x => x.User)
				.Where(x => x.WarehouseId == warehouseId &&
					   (x.Role == WarehouseRole.Manager ||
						x.Role == WarehouseRole.Shipper));

			// filter động
			if (role.HasValue)
			{
				query = query.Where(x => x.Role == role.Value);
			}

			return await query.ToListAsync();
		}
	}
}
