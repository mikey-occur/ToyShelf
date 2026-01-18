using Microsoft.AspNetCore.Authorization;
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
		//[Authorize(Roles = "Admin")]	
		public async Task<ActionResult<BaseResponse<IEnumerable<RoleResponse>>>> GetRoles([FromQuery] bool? isActive)
		{
			var result = await _roleService.GetRolesAsync(isActive);
			return BaseResponse<IEnumerable<RoleResponse>>.Ok(result, "Roles retrieved successfully");
		}

		[HttpGet("{id:guid}")]
		public async Task<BaseResponse<RoleResponse>> GetById(Guid id)
		{
			var role = await _roleService.GetByIdAsync(id);

			if (role == null)
				throw new AppException("Role not found", 404);

			return BaseResponse<RoleResponse>.Ok(role, "Role retrieved successfully");
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

		// ===== DISABLE (SOFT DELETE) =====

		[HttpPatch("{id:guid}/disable")]
		public async Task<ActionResult<ActionResponse>> Disable(Guid id)
		{
			await _roleService.DisableAsync(id);
			return ActionResponse.Ok("Role disabled successfully");
		}

		// ===== RESTORE =====

		[HttpPatch("{id:guid}/restore")]
		public async Task<ActionResult<ActionResponse>> Restore(Guid id)
		{
			await _roleService.RestoreAsync(id);
			return ActionResponse.Ok("Role restored successfully");
		}
	}
}
