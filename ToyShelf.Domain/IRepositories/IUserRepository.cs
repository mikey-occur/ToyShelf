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
		Task<List<User>> GetUsersAsync(
			bool? isActive,
			string? role);
		Task<List<User>> GetUsersByStoreOrPartnerAsync();
		Task<User?> GetUserWithPartnerAsync(Guid userId);
		Task<User?> GetUserWithWarehousesAsync(Guid userId);
		Task<User?> GetByEmailAsync(string email);
		Task<User?> GetPartnerAdminByPartnerIdAsync(Guid partnerId);
		Task<List<User>> GetUsersByRoleAndPartnerAsync(string roleName, Guid partnerId);
		Task<User?> GetUserWithRolesAsync(Guid userId);
		Task<List<User>> GetUsersWithWarehousesAsync();
	}
}
