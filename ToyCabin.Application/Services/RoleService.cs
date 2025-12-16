using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Role.Request;
using ToyCabin.Application.Models.Role.Response;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class RoleService : IRoleService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public RoleService(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
		{
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}

		// ===== GET =====

		public async Task<IEnumerable<RoleResponse>> GetAllAsync()
		{
			var roles = await _unitOfWork.Repository<Role>().GetAllAsync();
			return roles.Select(MapToResponse);
		}

		public async Task<IEnumerable<RoleResponse>> GetActiveAsync()
		{
			var roles = await _unitOfWork
				.Repository<Role>()
				.FindAsync(r => r.IsActive);

			return roles.Select(MapToResponse);
		}

		public async Task<IEnumerable<RoleResponse>> GetInactiveAsync()
		{
			var roles = await _unitOfWork
				.Repository<Role>()
				.FindAsync(r => !r.IsActive);

			return roles.Select(MapToResponse);
		}


		public async Task<RoleResponse?> GetByIdAsync(Guid id)
		{
			var role = await _unitOfWork.Repository<Role>().GetByIdAsync(id);
			return role == null ? null : MapToResponse(role);
		}

		// ===== CREATE =====

		public async Task<RoleResponse> CreateAsync(CreateRoleRequest request)
		{
			var role = new Role
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				Description = request.Description,
				IsActive = true,
				CreatedAt = _dateTime.UtcNow
			};

			await _unitOfWork.Repository<Role>().AddAsync(role);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(role);
		}

		// ===== UPDATE =====

		public async Task<RoleResponse?> UpdateAsync(Guid id, UpdateRoleRequest request)
		{
			var role = await _unitOfWork.Repository<Role>().GetByIdAsync(id);
			if (role == null)
				return null;

			role.Name = request.Name;
			role.Description = request.Description;
			role.UpdatedAt = _dateTime.UtcNow;

			_unitOfWork.Repository<Role>().Update(role);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(role);
		}

		// ===== DELETE (SOFT) =====

		public async Task<bool> DeleteAsync(Guid id)
		{
			var role = await _unitOfWork.Repository<Role>().GetByIdAsync(id);

			if (role == null || !role.IsActive)
				return false;

			role.IsActive = false;
			role.UpdatedAt = _dateTime.UtcNow;

			_unitOfWork.Repository<Role>().Update(role);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		// ===== RESTORE =====

		public async Task<bool> RestoreAsync(Guid id)
		{
			var role = await _unitOfWork.Repository<Role>().GetByIdAsync(id);

			if (role == null || role.IsActive)
				return false;

			role.IsActive = true;
			role.UpdatedAt = _dateTime.UtcNow;

			_unitOfWork.Repository<Role>().Update(role);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		// ===== MAPPER =====

		private static RoleResponse MapToResponse(Role role)
		{
			return new RoleResponse
			{
				Id = role.Id,
				Name = role.Name,
				Description = role.Description,
				IsActive = role.IsActive,
				CreatedAt = role.CreatedAt,
				UpdatedAt = role.UpdatedAt
			};
		}
	}
}
