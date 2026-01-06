using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Account.Request;
using ToyCabin.Application.Models.Account.Response;

namespace ToyCabin.Application.IServices
{
	public interface IAccountService
	{
		// ===== FLOW ACTIVATE =====
		Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request);
	    Task<CreateAccountResponse> CreatePartnerUserAsync(CreatePartnerUserRequest request, Guid partnerId, bool isPartnerAdmin);
		Task<ActivationOtpResponse> RequestActivateAccountAsync(string email);
		Task<ActivateAccountResponse> ActivateAccountAndSetPasswordAsync(ActivateAccountRequest request);

		// ===== FLOW LOCAL =====
		Task<RegisterResponse> RegisterLocalAsync(RegisterRequest request);
		Task<LoginResponse> LoginLocalAsync(LoginRequest request);

		// ===== FLOW LOGIN GG =====
		Task<LoginResponse> LoginGoogleAsync(string idToken);
		Task<SetLocalPasswordResponse> SetLocalPasswordAsync(Guid accountId, SetLocalPasswordRequest request);
	}
}
