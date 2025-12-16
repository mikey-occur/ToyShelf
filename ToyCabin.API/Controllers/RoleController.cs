using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Role.Request;
using ToyCabin.Application.Models.Role.Response;

namespace ToyCabin.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoleController : ControllerBase
	{
		private readonly IRoleService _roleService;

		public RoleController(IRoleService roleService)
		{
			_roleService = roleService;
		}

		// ===== GET =====

		[HttpGet]
		public async Task<BaseResponse<IEnumerable<RoleResponse>>> GetAll()
		{
			var result = await _roleService.GetAllAsync();
			return BaseResponse<IEnumerable<RoleResponse>>.Ok(result);
		}

		[HttpGet("active")]
		public async Task<BaseResponse<IEnumerable<RoleResponse>>> GetActive()
		{
			var result = await _roleService.GetActiveAsync();
			return BaseResponse<IEnumerable<RoleResponse>>.Ok(result);
		}

		[HttpGet("inactive")]
		public async Task<BaseResponse<IEnumerable<RoleResponse>>> GetInactive()
		{
			var result = await _roleService.GetInactiveAsync();
			return BaseResponse<IEnumerable<RoleResponse>>.Ok(result);
		}

		[HttpGet("{id:guid}")]
		public async Task<BaseResponse<RoleResponse>> GetById(Guid id)
		{
			var role = await _roleService.GetByIdAsync(id);

			if (role == null)
				throw new AppException("Role not found", 404);

			return BaseResponse<RoleResponse>.Ok(role);
		}

		// ===== CREATE =====

		[HttpPost]
		public async Task<BaseResponse<RoleResponse>> Create(
			[FromBody] CreateRoleRequest request)
		{
			var result = await _roleService.CreateAsync(request);
			return BaseResponse<RoleResponse>.Ok(result, "Role created successfully");
		}

		// ===== UPDATE =====

		[HttpPut("{id:guid}")]
		public async Task<BaseResponse<RoleResponse>> Update(
			Guid id,
			[FromBody] UpdateRoleRequest request)
		{
			var result = await _roleService.UpdateAsync(id, request);

			if (result == null)
				throw new AppException("Role not found", 404);

			return BaseResponse<RoleResponse>.Ok(result, "Role updated successfully");
		}

		// ===== DELETE (SOFT) =====

		[HttpDelete("{id:guid}")]
		public async Task<BaseResponse<bool>> Delete(Guid id)
		{
			var success = await _roleService.DeleteAsync(id);

			if (!success)
				throw new AppException("Role not found or already inactive", 400);

			return BaseResponse<bool>.Ok(true, "Role deactivated successfully");
		}

		// ===== RESTORE =====

		[HttpPut("{id:guid}/restore")]
		public async Task<BaseResponse<bool>> Restore(Guid id)
		{
			var success = await _roleService.RestoreAsync(id);

			if (!success)
				throw new AppException("Role not found or already active", 400);

			return BaseResponse<bool>.Ok(true, "Role restored successfully");
		}
	}
}
