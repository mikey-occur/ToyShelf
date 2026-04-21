using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.User.Request;
using ToyShelf.Application.Models.User.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IUserService
	{
		Task<List<UserProfileResponse>> GetUsersAsync(
			bool? isActive,
			string? role,
			string? roleBusiness);
		Task<List<UserResponse>> GetUsersByStoreOrPartnerAsync(
			GetUserByStoreOrPartnerRequest request,
			ICurrentUser currentUser);
		Task<PartnerDetailByUserResponse> GetPartnerDetailByUserAsync(
			GetPartnerDetailByUserRequest request);

		Task<List<WarehouseDetailByUserResponse>> GetWarehouseUsersAsync(
			GetWarehouseUsersRequest request);

		Task<List<WarehouseDetailByUserResponse>> GetWarehouseDetailByUserAsync(Guid userId);

		Task<UserProfileResponse> GetProfileByUserIdAsync(Guid userId);
		Task<UserProfileResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request);
		Task DisableUserAsync(Guid userId);   
		Task RestoreUserAsync(Guid userId);  
	}
}
