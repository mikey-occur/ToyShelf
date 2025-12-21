using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Auth;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Account.Request;
using ToyCabin.Application.Notifications;
using ToyCabin.Application.Security;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class AccountService : IAccountService
	{
		private readonly IAccountRepository _accountRepository;
		private readonly IUserRepository _userRepository;
		private readonly IPasswordHasher _passwordHasher;
		private readonly ITokenService _tokenService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPasswordResetOtpRepository _otpRepo;
		private readonly IEmailService _emailService;
		public AccountService(IAccountRepository accountRepository, 
							  IUserRepository userRepository, 
							  IPasswordHasher passwordHasher, 
							  ITokenService tokenService,
							  IUnitOfWork unitOfWork,
							  IPasswordResetOtpRepository otpRepo,
							  IEmailService emailService)
		{ 
			_accountRepository = accountRepository;
			_userRepository = userRepository;
			_passwordHasher = passwordHasher;
			_tokenService = tokenService;
			_unitOfWork = unitOfWork;
			_otpRepo = otpRepo;
			_emailService = emailService;
		}

		// ===== ADMIN =====
		public async Task CreateAccountAsync(CreateAccountRequest request)
		{
			if (await _accountRepository.ExistsLocalAccountByEmailAsync(request.Email))
				throw new Exception("Email already exists");

			var user = new User
			{
				Id = Guid.NewGuid(),
				Email = request.Email,
				FullName = request.FullName,
				CreatedAt = DateTime.UtcNow
			};

			await _userRepository.AddAsync(user);

			var account = new Account
			{
				UserId = user.Id,
				Provider = AuthProvider.LOCAL,
				IsFirstLogin = true,
				CreatedAt = DateTime.UtcNow
			};

			await _accountRepository.AddAsync(account);

			foreach (var roleId in request.RoleIds)
			{
				await _unitOfWork.Repository<AccountRole>()
					.AddAsync(new AccountRole
					{
						AccountId = account.Id,
						RoleId = roleId
					});
			}

			await _unitOfWork.SaveChangesAsync();
		}

		// Activate account flow
		public async Task RequestActivateAccountAsync(string email)
		{
			var account = await _accountRepository
				.GetLocalAccountByEmailAsync(email);

			if (account == null)
				throw new Exception("Account not found");

			if (!account.IsFirstLogin)
				throw new Exception("Account already activated");

			// invalidate OTP cũ nếu có
			var oldOtps = await _otpRepo.FindAsync(o =>
				o.AccountId == account.Id &&
				!o.IsUsed &&
				o.Purpose == OtpPurpose.ACTIVATE_ACCOUNT);

			foreach (var o in oldOtps)
				o.IsUsed = true;

			var otpCode = Random.Shared.Next(100000, 999999).ToString();
			var expiredAt = DateTime.UtcNow.AddMinutes(10);

			var otp = new PasswordResetOtp
			{
				Account = account,
				OtpCode = otpCode,
				Purpose = OtpPurpose.ACTIVATE_ACCOUNT,
				ExpiredAt = expiredAt,
				CreatedAt = DateTime.UtcNow
			};

			await _otpRepo.AddAsync(otp);
			await _unitOfWork.SaveChangesAsync();

			await _emailService.SendOtpAsync(
				toEmail: account.User.Email,
				otpCode: otpCode,
				purpose: OtpPurpose.ACTIVATE_ACCOUNT,
				expiredAt: expiredAt,
				fullName: account.User.FullName
			);
		}


		public async Task ActivateAccountAndSetPasswordAsync(ActivateAccountRequest request)
		{
			var otp = await _otpRepo.GetWithAccountAsync(request.OtpCode, OtpPurpose.ACTIVATE_ACCOUNT);

			if (otp == null)
				throw new Exception("Invalid or expired OTP");

			var account = otp.Account!; 

			var salt = _passwordHasher.GenerateSalt();
			account.Salt = salt;
			account.PasswordHash = _passwordHasher.Hash(request.NewPassword, salt);
			account.IsFirstLogin = false;
			account.LastLoginAt = DateTime.UtcNow;

			otp.IsUsed = true;

			_accountRepository.Update(account);
			await _unitOfWork.SaveChangesAsync();
		}


		// ===== USER =====

		public async Task RegisterLocalAsync(	string email, string fullName, string password)
		{
			if (await _accountRepository.ExistsLocalAccountByEmailAsync(email))
				throw new Exception("Email already registered");

			var salt = _passwordHasher.GenerateSalt();

			var user = new User
			{
				Email = email,
				FullName = fullName,
				CreatedAt = DateTime.UtcNow
			};

			var account = new Account
			{
				User = user,
				Provider = AuthProvider.LOCAL,
				Salt = salt,
				PasswordHash = _passwordHasher.Hash(password, salt),
				IsFirstLogin = false,
				CreatedAt = DateTime.UtcNow
			};

			await _accountRepository.AddAsync(account);
		}

		public async Task<string> LoginLocalAsync(string email, string password)
		{
			var account = await _accountRepository
				.GetLocalAccountByEmailAsync(email);

			if (account == null || account.PasswordHash == null)
				throw new Exception("Invalid credentials");

			if (!_passwordHasher.Verify(
				password,
				account.Salt!,
				account.PasswordHash))
				throw new Exception("Invalid credentials");

			account.LastLoginAt = DateTime.UtcNow;
			_accountRepository.Update(account);

			return _tokenService.GenerateAccessToken(account);
		}
	}
}
