using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.User.Request;
using ToyShelf.Application.Models.User.Response;

namespace ToyShelf.Application.IServices
{
	public interface IUserService
	{
		Task<List<UserProfileResponse>> GetUsersAsync(bool? isActive);
		Task<UserProfileResponse> GetProfileByUserIdAsync(Guid userId);
		Task<UserProfileResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request);
		Task DisableUserAsync(Guid userId);   
		Task RestoreUserAsync(Guid userId);  
	}
}
