using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.Account.Request;

namespace ToyCabin.Application.IServices
{
	public interface IAccountService
	{
		// ===== ADMIN =====
		Task CreateAccountAsync(CreateAccountRequest request);

		Task RequestActivateAccountAsync(string email);

		Task ActivateAccountAndSetPasswordAsync(ActivateAccountRequest request);

		// ===== USER =====
		Task RegisterLocalAsync(
			string email,
			string fullName,
			string password);

		Task<string> LoginLocalAsync(
			string email,
			string password);
	}
}
