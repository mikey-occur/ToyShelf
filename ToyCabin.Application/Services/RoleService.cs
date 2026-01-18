using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Common;
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
		private readonly IRoleRepository _roleRepository;

		public RoleService(IUnitOfWork unitOfWork, IDateTimeProvider dateTime, IRoleRepository roleRepository)
		{
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_roleRepository = roleRepository;
		}

		// ===== GET =====

		public async Task<IEnumerable<RoleResponse>> GetRolesAsync(bool? isActive)
		{
			var roles = await _roleRepository.GetRolesAsync(isActive);
			return roles.Select(MapToResponse);
		}

		public async Task<RoleResponse?> GetByIdAsync(Guid id)
		{
			var role = await _roleRepository.GetByIdAsync(id);
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
			await _roleRepository.AddAsync(role);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(role);
		}

		// ===== UPDATE =====

		public async Task<RoleResponse?> UpdateAsync(Guid id, UpdateRoleRequest request)
		{
			var role = await _roleRepository.GetByIdAsync(id);
			if (role == null)
				return null;

			role.Name = request.Name;
			role.Description = request.Description;
			role.UpdatedAt = _dateTime.UtcNow;

			_roleRepository.Update(role);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(role);
		}

		// ===== DISABLE / RESTORE =====
		public async Task DisableAsync(Guid id)
		{
			var role = await _roleRepository.GetByIdAsync(id)
				?? throw new AppException("Role not found", 404);

			if (!role.IsActive)
				throw new AppException("Role already inactive", 400);

			role.IsActive = false;
			role.UpdatedAt = _dateTime.UtcNow;

			_roleRepository.Update(role);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task RestoreAsync(Guid id)
		{
			var role = await _roleRepository.GetByIdAsync(id)
				?? throw new AppException("Role not found", 404);

			if (role.IsActive)
				throw new AppException("Role already active", 400);

			role.IsActive = true;
			role.UpdatedAt = _dateTime.UtcNow;

			_roleRepository.Update(role);
			await _unitOfWork.SaveChangesAsync();
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
