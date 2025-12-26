using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.User.Request;
using ToyCabin.Application.Models.User.Response;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IUnitOfWork _unitOfWork;

		public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
		{
			_userRepository = userRepository;
			_unitOfWork = unitOfWork;
		}

		// ================= GET =================

		public async Task<List<UserProfileResponse>> GetActiveUsersAsync()
		{
			var users = await _userRepository.GetActiveUsersAsync();
			return users.Select(MapToProfile).ToList();
		}

		public async Task<List<UserProfileResponse>> GetInactiveUsersAsync()
		{
			var users = await _userRepository.GetInactiveUsersAsync();
			return users.Select(MapToProfile).ToList();
		}

		public async Task<UserProfileResponse> GetProfileByUserIdAsync(Guid userId)
		{
			var user = await _userRepository.GetByIdAsync(userId)
				?? throw new Exception("User not found");

			return MapToProfile(user);
		}

		// ================= UPDATE =================

		public async Task<UserProfileResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request)
		{
			var user = await _userRepository.GetByIdAsync(userId)
				?? throw new Exception("User not found");

			user.FullName = request.FullName;
			user.AvatarUrl = request.AvatarUrl;
			user.UpdatedAt = DateTime.UtcNow;

			_userRepository.Update(user);
			await _unitOfWork.SaveChangesAsync();

			return MapToProfile(user);
		}

		// ================= DELETE / RESTORE =================

		public async Task DisableUserAsync(Guid userId)
		{
			var user = await _userRepository.GetByIdAsync(userId)
				?? throw new Exception("User not found");

			if (!user.IsActive)
				throw new Exception("User already inactive");

			user.IsActive = false;
			user.UpdatedAt = DateTime.UtcNow;

			_userRepository.Update(user);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task RestoreUserAsync(Guid userId)
		{
			var user = await _userRepository.GetByIdAsync(userId)
				?? throw new Exception("User not found");

			if (user.IsActive)
				throw new Exception("User already active");

			user.IsActive = true;
			user.UpdatedAt = DateTime.UtcNow;

			_userRepository.Update(user);
			await _unitOfWork.SaveChangesAsync();
		}

		// ================= MAPPER =================
		private static UserProfileResponse MapToProfile(User user)
		{
			return new UserProfileResponse
			{
				Id = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				AvatarUrl = user.AvatarUrl,
				IsActive = user.IsActive,
				CreatedAt = user.CreatedAt
			};
		}
	}
}
