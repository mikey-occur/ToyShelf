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
		// ===== ADMIN =====
		Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request);
		Task<ActivationOtpResponse> RequestActivateAccountAsync(string email);
		Task<ActivateAccountResponse> ActivateAccountAndSetPasswordAsync(ActivateAccountRequest request);

		// ===== USER =====
		Task<RegisterResponse> RegisterLocalAsync(RegisterRequest request);
		Task<LoginResponse> LoginLocalAsync(LoginRequest request);
		Task<LoginResponse> LoginGoogleAsync(string idToken);
	}
}
