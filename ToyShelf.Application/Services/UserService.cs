using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.User.Request;
using ToyShelf.Application.Models.User.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
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

		public async Task<List<UserProfileResponse>> GetUsersAsync(bool? isActive)
		{
			var users = await _userRepository.GetUsersAsync(isActive);
			return users.Select(MapToProfile).ToList();
		}

		public async Task<List<UserResponse>> GetUsersByStoreOrPartnerAsync(
			GetUserByStoreOrPartnerRequest request,
			ICurrentUser currentUser)
		{
			var users = await _userRepository.GetUsersByStoreOrPartnerAsync();

			var query = users.AsQueryable();

			// ===== PartnerAdmin chỉ thấy partner của mình =====

			if (currentUser.IsPartnerAdmin())
			{
				query = query.Where(u =>
					u.PartnerId == currentUser.PartnerId ||
					u.UserStores.Any(us => us.Store.PartnerId == currentUser.PartnerId));
			}

			// ===== Admin filter theo Partner =====

			if (currentUser.IsAdmin() && request.PartnerId.HasValue)
			{
				query = query.Where(u =>
					u.PartnerId == request.PartnerId.Value ||
					u.UserStores.Any(us => us.Store.PartnerId == request.PartnerId.Value));
			}

			// ===== Filter theo Store =====

			if (request.StoreId.HasValue)
			{
				query = query.Where(u =>
					u.UserStores.Any(us => us.StoreId == request.StoreId.Value));
			}

			// ===== Filter theo StoreRole =====

			if (request.StoreRole.HasValue)
			{
				query = query.Where(u =>
					u.UserStores.Any(us => us.StoreRole == request.StoreRole.Value));
			}

			return query
				.AsEnumerable()
				.SelectMany(u =>
				{
					if (u.UserStores.Any())
					{
						return u.UserStores.Select(us => new UserResponse
						{
							UserId = u.Id,
							FullName = u.FullName,
							Email = u.Email,
							PartnerId = us.Store.PartnerId,
							StoreId = us.StoreId,
							StoreName = us.Store.Name,
							StoreRole = us.StoreRole
						});
					}

					return new List<UserResponse>
					{
						new UserResponse
						{
							UserId = u.Id,
							FullName = u.FullName,
							Email = u.Email,
							PartnerId = u.PartnerId,
							StoreId = null,
							StoreName = null,
							StoreRole = null
						}
					};
				})
				.ToList();
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
