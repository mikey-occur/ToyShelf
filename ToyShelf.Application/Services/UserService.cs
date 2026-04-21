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

		public async Task<List<UserProfileResponse>> GetUsersAsync(
			bool? isActive,
			string? role,
			string? roleBusiness)
		{
			var users = await _userRepository.GetUsersAsync(isActive, role);

			var mapped = users.Select(MapToProfile);

			if (!string.IsNullOrEmpty(roleBusiness))
			{
				mapped = mapped.Where(u =>
					u.BusinessRole == roleBusiness
				);
			}

			return mapped.ToList();
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

			// ===== Filter theo Status =====
			if (request.IsActive.HasValue)
			{
				query = query.Where(u => u.IsActive == request.IsActive.Value);
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
							StoreRole = us.StoreRole,
							IsActive = u.IsActive
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
							IsActive = u.IsActive,
							StoreId = null,
							StoreName = null,
							StoreRole = null
						}
					};
				})
				.ToList();
		}

		public async Task<PartnerDetailByUserResponse> GetPartnerDetailByUserAsync(
			GetPartnerDetailByUserRequest request)
		{
			var user = await _userRepository.GetUserWithPartnerAsync(request.UserId);

			if (user == null)
				throw new Exception("User not found");

			return new PartnerDetailByUserResponse
			{
				UserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				PartnerId = user.PartnerId,
				CompanyName = user.Partner?.CompanyName,
				PartnerIsActive = user.Partner?.IsActive
			};
		}

		public async Task<List<WarehouseDetailByUserResponse>> GetWarehouseUsersAsync(
			GetWarehouseUsersRequest request)
		{
			var users = await _userRepository.GetUsersWithWarehousesAsync();

			var query = users.AsQueryable();

			// Filter theo Warehouse
			if (request.WarehouseId.HasValue)
			{
				query = query.Where(u =>
					u.UserWarehouses.Any(uw => uw.WarehouseId == request.WarehouseId.Value));
			}

			// Filter theo Role
			if (request.Role.HasValue)
			{
				query = query.Where(u =>
					u.UserWarehouses.Any(uw => uw.Role == request.Role.Value));
			}

			return query
				.AsEnumerable()
				.SelectMany(u =>
				{
					if (u.UserWarehouses.Any())
					{
						return u.UserWarehouses.Select(uw => new WarehouseDetailByUserResponse
						{
							UserId = u.Id,
							Email = u.Email,
							FullName = u.FullName,
							UserIsActive = u.IsActive,
							UserCreatedAt = u.CreatedAt,

							WarehouseId = uw.WarehouseId,
							WarehouseName = uw.Warehouse.Name,
							WarehouseIsActive = uw.Warehouse.IsActive,
							WarehouseCreatedAt = uw.Warehouse.CreatedAt,
							WarehouseRole = uw.Role,
							WarehouseLocationIds = uw.Warehouse.InventoryLocations
								.Select(x => x.Id)
								.ToList()
						});
					}

					return new List<WarehouseDetailByUserResponse>();
				})
				.ToList();
		}

		public async Task<List<WarehouseDetailByUserResponse>> GetWarehouseDetailByUserAsync(Guid userId)
		{
			var user = await _userRepository.GetUserWithWarehousesAsync(userId);

			if (user == null)
				throw new Exception("User not found");

			if (!user.UserWarehouses.Any())
			{
				return new List<WarehouseDetailByUserResponse>
		{
			new WarehouseDetailByUserResponse
			{
				UserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				UserIsActive = user.IsActive,
				UserCreatedAt = user.CreatedAt,

				WarehouseId = null,
				WarehouseName = null,
				WarehouseIsActive = null,
				WarehouseCreatedAt = null,
				WarehouseRole = null,
				WarehouseLocationIds = new List<Guid>()
			}
		};
			}

			// Có warehouse → map giống hệt hàm trên
			return user.UserWarehouses.Select(uw => new WarehouseDetailByUserResponse
			{
				UserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				UserIsActive = user.IsActive,
				UserCreatedAt = user.CreatedAt,

				WarehouseId = uw.WarehouseId,
				WarehouseName = uw.Warehouse?.Name,
				WarehouseIsActive = uw.Warehouse?.IsActive,
				WarehouseCreatedAt = uw.Warehouse?.CreatedAt,
				WarehouseRole = uw.Role,

				WarehouseLocationIds = uw.Warehouse?.InventoryLocations?
					.Select(x => x.Id)
					.ToList() ?? new List<Guid>()

			}).ToList();
		}


		public async Task<UserProfileResponse> GetProfileByUserIdAsync(Guid userId)
		{
			var user = await _userRepository.GetUserWithRolesAsync(userId)
				?? throw new Exception("User not found");

			return MapToProfile(user);
		}

		// ================= UPDATE =================

		public async Task<UserProfileResponse> UpdateUserAsync(Guid userId, UpdateUserRequest request)
		{
			var user = await _userRepository.GetUserWithRolesAsync(userId)
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
			var user = await _userRepository.GetUserWithRolesAsync(userId)
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
			var user = await _userRepository.GetUserWithRolesAsync(userId)
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
				CreatedAt = user.CreatedAt,
				Roles = user.Accounts != null ? user.Accounts
				.SelectMany(a => a.AccountRoles)
				.Select(ar => ar.Role?.Name ?? "")
				.Where(name => !string.IsNullOrEmpty(name))
				.Distinct()
				.ToList() : new List<string>(),

				
				BusinessRole = DetermineBusinessRole(user)
			};
		}

		private static string DetermineBusinessRole(User user)
		{
			var isAdmin = user.Accounts?
				.SelectMany(a => a.AccountRoles)
				.Any(ar => ar.Role.Name == "Admin") ?? false;

			if (isAdmin)
			{
				return "admin";
			}

			var warehouseAssignment = user.UserWarehouses?.FirstOrDefault();
			if (warehouseAssignment != null)
			{
				return $"warehouse_{warehouseAssignment.Role.ToString().ToLower()}";
			}

			var storeAssignment = user.UserStores?.FirstOrDefault();
			if (storeAssignment != null)
			{
				return storeAssignment.StoreRole == StoreRole.Manager
					? "partner_manager"
					: "partner_staff";
			}

			if (user.PartnerId != null)
			{
				return "partner_admin";
			}


			return "customer";
		}
	}
}
