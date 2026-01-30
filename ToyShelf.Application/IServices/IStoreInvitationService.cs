using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.UserStore.Request;

namespace ToyShelf.Application.IServices
{
	public interface IStoreInvitationService
	{
		Task<bool> InviteUserAsync(
			InviteUserToStoreRequest request,
			ICurrentUser currentUser);
		Task<bool> AcceptInvitationAsync(Guid invitationId, Guid currentUserId);
		Task<bool> RejectInvitationAsync(Guid invitationId, Guid currentUserId);
	}
}	
