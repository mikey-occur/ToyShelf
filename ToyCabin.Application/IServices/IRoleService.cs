using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Role.Request;
using ToyCabin.Application.Models.Role.Response;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Application.IServices
{
	public interface IRoleService
	{
		Task<IEnumerable<RoleResponse>> GetAllAsync();       
		Task<IEnumerable<RoleResponse>> GetActiveAsync();
		Task<IEnumerable<RoleResponse>> GetInactiveAsync();
		Task<RoleResponse?> GetByIdAsync(Guid id);
		Task<RoleResponse> CreateAsync(CreateRoleRequest request);
		Task<RoleResponse?> UpdateAsync(Guid id, UpdateRoleRequest request);
		Task<bool> DeleteAsync(Guid id);  
		// khôi phục
		Task<bool> RestoreAsync(Guid id); 
	}
}
