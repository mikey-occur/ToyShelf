using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.UserStore.Request;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class StoreInvitationService : IStoreInvitationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IStoreInvitationRepository _invitationRepo;
		private readonly IUserStoreRepository _userStoreRepo;
		private readonly IUserRepository _userRepository;

		public StoreInvitationService(
			IUnitOfWork unitOfWork,
			IStoreInvitationRepository invitationRepo,
			IUserStoreRepository userStoreRepo,
			IUserRepository userRepository)
		{
			_unitOfWork = unitOfWork;
			_invitationRepo = invitationRepo;
			_userStoreRepo = userStoreRepo;
			_userRepository = userRepository;
		}

		// ================ FLOW INVITATION =================
		public async Task<bool> InviteUserAsync(
			InviteUserToStoreRequest request,
			ICurrentUser currentUser)
		{
			var isPartnerAdmin = currentUser.IsPartnerAdmin();
			var isPartner = currentUser.IsPartner();

			if (!isPartnerAdmin && !isPartner)
				throw new ForbiddenException();

			// Find user by email
			var user = await _userRepository.GetByEmailAsync(request.Email);
			if (user == null)
				throw new Exception("User with this email does not exist or inactive");

			// User already in store
			if (await _userStoreRepo.AnyAsync(x =>
				x.StoreId == request.StoreId &&
				x.UserId == user.Id &&
				x.IsActive))
				throw new Exception("User already in store");

			// Partner role checks
			if (isPartner && !isPartnerAdmin)
			{
				var inviterStore = await _userStoreRepo.GetAsync(x =>
					x.UserId == currentUser.UserId &&
					x.StoreId == request.StoreId &&
					x.IsActive);

				if (inviterStore == null)
					throw new ForbiddenException("User not in store");

				if (inviterStore.StoreRole != StoreRole.Manager)
					throw new ForbiddenException("Only Store Manager can invite users");

				// Partner Manager chỉ được mời Staff
				if (request.StoreRole == StoreRole.Manager)
					throw new ForbiddenException("Partner cannot invite Manager");
			}

			// Partner Admin Checks RUle
			if (isPartnerAdmin && request.StoreRole == StoreRole.Manager)
			{
				var hasManager = await _userStoreRepo.AnyAsync(x =>
					x.StoreId == request.StoreId &&
					x.StoreRole == StoreRole.Manager &&
					x.IsActive);

				if (hasManager)
					throw new Exception("Store already has a Manager");
			}

			// invitation already exists
			if (await _invitationRepo.AnyAsync(x =>
				x.StoreId == request.StoreId &&
				x.UserId == user.Id &&
				x.Status == InvitationStatus.Pending))
				throw new Exception("Invitation already exists");

			// Create invitation
			await _invitationRepo.AddAsync(new StoreInvitation
			{
				Id = Guid.NewGuid(),
				StoreId = request.StoreId,
				UserId = user.Id,
				InvitedByUserId = currentUser.UserId,
				StoreRole = request.StoreRole, // Manager | Staff theo rule
				Status = InvitationStatus.Pending,
				CreatedAt = DateTime.UtcNow,
				ExpiredAt = DateTime.UtcNow.AddDays(7)
			});

			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		// Accept invitation
		public async Task<bool> AcceptInvitationAsync(Guid invitationId, Guid currentUserId)
		{
			var invitation = await _invitationRepo.GetAsync(x => x.Id == invitationId)
				?? throw new Exception("Invitation not found");

			if (invitation.UserId != currentUserId)
				throw new ForbiddenException();

			if (invitation.Status != InvitationStatus.Pending)
				throw new Exception("Invitation is not valid");

			if (invitation.ExpiredAt < DateTime.UtcNow)
				throw new Exception("Invitation expired");

			invitation.Status = InvitationStatus.Accepted;
			invitation.ExpiredAt = null;

			await _userStoreRepo.AddAsync(new UserStore
			{
				UserId = invitation.UserId,
				StoreId = invitation.StoreId,
				StoreRole = invitation.StoreRole,
				IsActive = true
			});

			await _unitOfWork.SaveChangesAsync();
			return true;

		}

		// Reject invitation
		public async Task<bool> RejectInvitationAsync(Guid invitationId, Guid currentUserId)
		{
			var invitation = await _invitationRepo.GetAsync(x => x.Id == invitationId)
				?? throw new Exception("Invitation not found");

			if (invitation.UserId != currentUserId)
				throw new ForbiddenException();

			if (invitation.Status != InvitationStatus.Pending)
				throw new Exception("Invitation is not valid");

			invitation.Status = InvitationStatus.Rejected;
			invitation.ExpiredAt = null;

			await _unitOfWork.SaveChangesAsync();

			return true;
		}
	}
}
