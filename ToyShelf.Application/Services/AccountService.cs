using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Account.Request;
using ToyShelf.Application.Models.Account.Response;
using ToyShelf.Application.Models.UserWarehouse.Request;
using ToyShelf.Application.Notifications;
using ToyShelf.Application.Security;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
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
		private readonly IRoleRepository _roleRepository;
		public AccountService(IAccountRepository accountRepository, 
							  IUserRepository userRepository, 
							  IPasswordHasher passwordHasher, 
							  ITokenService tokenService,
							  IUnitOfWork unitOfWork,
							  IPasswordResetOtpRepository otpRepo,
							  IEmailService emailService,
							  IRoleRepository roleRepository)
		{ 
			_accountRepository = accountRepository;
			_userRepository = userRepository;
			_passwordHasher = passwordHasher;
			_tokenService = tokenService;
			_unitOfWork = unitOfWork;
			_otpRepo = otpRepo;
			_emailService = emailService;
			_roleRepository = roleRepository;
		}

		// ================= FLOW ACTIVATE =================
		public async Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request)
		{
			if (await _accountRepository.ExistsLocalAccountByEmailAsync(request.Email))
				throw new AppException("Email already exists", 409);

			var user = new User
			{
				Id = Guid.NewGuid(),
				PartnerId = request.PartnerId,
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

			return new CreateAccountResponse
			{
				Email = user.Email,
				FullName = user.FullName
			};
		}
		public async Task<CreateAccountResponse> CreatePartnerUserAsync(
			CreatePartnerUserRequest request,
			Guid partnerId,
			bool isPartnerAdmin)
		{
			if (!isPartnerAdmin)
				throw new ForbiddenException();

			if (await _accountRepository.ExistsLocalAccountByEmailAsync(request.Email))
				throw new AppException("Email already exists", 409);

			var user = new User
			{
				Id = Guid.NewGuid(),
				PartnerId = partnerId,
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

			var partnerRoleId = await _roleRepository.GetByNameAsync("Partner")
				?? throw new AppException("Default role Partner not found", 404);

			await _unitOfWork.Repository<AccountRole>()
				.AddAsync(new AccountRole
				{
					AccountId = account.Id,
					RoleId = partnerRoleId.Id,
				});

			await _unitOfWork.SaveChangesAsync();

			return new CreateAccountResponse
			{
				Email = user.Email,
				FullName = user.FullName
			};
		}
		public async Task<CreateAccountResponse> CreateWarehouseUserAsync(
			CreateWarehouseUserRequest request,
			ICurrentUser currentUser)
		{
			if (!currentUser.IsAdmin())
				throw new ForbiddenException();

			if (await _accountRepository.ExistsLocalAccountByEmailAsync(request.Email))
				throw new AppException("Email already exists", 409);

			var warehouse = await _unitOfWork.Repository<Warehouse>()
				.GetByIdAsync(request.WarehouseId);

			if (warehouse == null)
				throw new AppException("Warehouse not found", 404);

			// Create User
			var user = new User
			{
				Id = Guid.NewGuid(),
				Email = request.Email,
				FullName = request.FullName,
				CreatedAt = DateTime.UtcNow
			};

			await _userRepository.AddAsync(user);

			// Create Account
			var account = new Account
			{
				UserId = user.Id,
				Provider = AuthProvider.LOCAL,
				IsFirstLogin = true,
				CreatedAt = DateTime.UtcNow
			};

			await _accountRepository.AddAsync(account);

			// SYSTEM ROLE (Warehouse)
			var warehouseRole = await _roleRepository.GetByNameAsync("Warehouse")
				?? throw new AppException("Role Warehouse not found", 404);

			await _unitOfWork.Repository<AccountRole>()
				.AddAsync(new AccountRole
				{
					AccountId = account.Id,
					RoleId = warehouseRole.Id
				});

			// BUSINESS ROLE (Manager / Shipper)
			await _unitOfWork.Repository<UserWarehouse>()
				.AddAsync(new UserWarehouse
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					WarehouseId = request.WarehouseId,
					Role = request.Role,
					CreatedAt = DateTime.UtcNow
				});

			await _unitOfWork.SaveChangesAsync();

			return new CreateAccountResponse
			{
				Email = user.Email,
				FullName = user.FullName
			};
		}

		public async Task<ActivationOtpResponse> RequestActivateAccountAsync(string email)
		{
			var account = await _accountRepository
				.GetLocalAccountByEmailAsync(email);

			if (account == null)
				throw new AppException("Account not found",404);

			if (!account.IsFirstLogin)
				throw new AppException("Account already activated", 400);

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

			return new ActivationOtpResponse
			{
				Email = account.User.Email,
				ExpiredAt = expiredAt
			};
		}
		public async Task<ActivateAccountResponse> ActivateAccountAndSetPasswordAsync(ActivateAccountRequest request)
		{
			if (request.NewPassword != request.ConfirmPassword)
				throw new AppException("Password and ConfirmPassword do not match", 400);

			var otp = await _otpRepo
				.GetWithAccountAsync(request.OtpCode, OtpPurpose.ACTIVATE_ACCOUNT, request.Email);

			if (otp == null)
				throw new AppException("Invalid or expired OTP", 400);


			if (otp.ExpiredAt < DateTime.UtcNow || otp.IsUsed)
				throw new AppException("Invalid or expired OTP", 400);

			var account = otp.Account!;

			// extra safety: chắc chắn đúng account local & chưa activate
			if (!account.IsFirstLogin)
				throw new AppException("Account already activated", 400);

			var salt = _passwordHasher.GenerateSalt();
			account.Salt = salt;
			account.PasswordHash = _passwordHasher.Hash(request.NewPassword, salt);
			account.IsFirstLogin = false;
			account.LastLoginAt = DateTime.UtcNow;

			otp.IsUsed = true;

			_accountRepository.Update(account);
			await _unitOfWork.SaveChangesAsync();

			return new ActivateAccountResponse
			{
				Email = account.User.Email,
				LastLoginAt = account.LastLoginAt.Value
			};
		}

		// ================= FLOW LOCAL =================
		public async Task<RegisterResponse> RegisterLocalAsync(RegisterRequest request)
		{
			if (request.Password != request.ConfirmPassword)
				throw new AppException("Password and ConfirmPassword do not match", 400);

			if (await _accountRepository.ExistsLocalAccountByEmailAsync(request.Email))
				throw new AppException("Email already registered", 400);

			var salt = _passwordHasher.GenerateSalt();

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
				Salt = salt,
				PasswordHash = _passwordHasher.Hash(request.Password, salt),
				IsFirstLogin = false,
				CreatedAt = DateTime.UtcNow
			};

			await _accountRepository.AddAsync(account);

			var customerRole = await _roleRepository.GetByNameAsync("Customer")
						?? throw new AppException("Default role Customer not found", 404);

			await _unitOfWork.Repository<AccountRole>()
				.AddAsync(new AccountRole
				{
					AccountId = account.Id,
					RoleId = customerRole.Id
				});

			await _unitOfWork.SaveChangesAsync();

			return new RegisterResponse
			{
				UserId = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				CreatedAt = user.CreatedAt
			};
		}
		public async Task<LoginResponse> LoginLocalAsync(LoginRequest request)
		{
			var account = await _accountRepository.GetLocalAccountByEmailAsync(request.Email);

			if (account == null || account.PasswordHash == null)
				throw new AppException("Invalid credentials", 400);

			if (!_passwordHasher.Verify(request.Password, account.Salt!, account.PasswordHash))
				throw new AppException("Invalid credentials", 400);

			account.LastLoginAt = DateTime.UtcNow;
			_accountRepository.Update(account);
			await _unitOfWork.SaveChangesAsync();

			var accessToken = await _tokenService.GenerateAccessTokenAsync(account);

			return new LoginResponse
			{
				AccessToken = accessToken,
				Roles = account.AccountRoles
						.Select(rn => rn.Role.Name)
						.ToList(),
				LastLoginAt = account.LastLoginAt.Value
			};
		}

		// ================= FLOW LOGIN GG =================
		public async Task<LoginResponse> LoginGoogleAsync(string idToken)
		{
			// 1. Verify Google token
			var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
			var email = payload.Email;

			// 2. Tìm Google account
			var account = await _accountRepository
				.GetAccountByEmailAndProviderAsync(email, AuthProvider.GOOGLE);

			if (account == null)
			{
				// 3. Tìm Local account
				var localAccount = await _accountRepository
					.GetAccountByEmailAndProviderAsync(email, AuthProvider.LOCAL);

				if (localAccount != null)
				{
					// User đã tồn tại -> chỉ thêm Google account
					account = new Account
					{
						Id = Guid.NewGuid(),
						UserId = localAccount.UserId,
						Provider = AuthProvider.GOOGLE,
						IsFirstLogin = false,
						CreatedAt = DateTime.UtcNow
					};

					await _accountRepository.AddAsync(account);
				}
				else
				{
					//  User mới hoàn toàn (Google first)
					var user = new User
					{
						Id = Guid.NewGuid(),
						Email = email,
						FullName = payload.Name,
						CreatedAt = DateTime.UtcNow
					};
					await _userRepository.AddAsync(user);

					account = new Account
					{
						Id = Guid.NewGuid(),
						UserId = user.Id,
						Provider = AuthProvider.GOOGLE,
						IsFirstLogin = true,
						CreatedAt = DateTime.UtcNow
					};
					await _accountRepository.AddAsync(account);

					var customerRole = await _roleRepository.GetByNameAsync("Customer")
						?? throw new AppException("Default role Customer not found", 404);

					await _unitOfWork.Repository<AccountRole>()
						.AddAsync(new AccountRole
						{
							AccountId = account.Id,
							RoleId = customerRole.Id
						});
				}

				await _unitOfWork.SaveChangesAsync();
			}

			// 4. Update login time
			account.LastLoginAt = DateTime.UtcNow;
			_accountRepository.Update(account);
			await _unitOfWork.SaveChangesAsync();

			// 5. Generate token (role lấy theo USER)
			var accessToken = await _tokenService.GenerateAccessTokenAsync(account);

			return new LoginResponse
			{
				AccessToken = accessToken,
				Roles = account.AccountRoles
						.Select(nr => nr.Role.Name)
						.ToList(),
				LastLoginAt = account.LastLoginAt.Value
			};
		}
		public async Task<SetLocalPasswordResponse> SetLocalPasswordAsync(Guid accountId, SetLocalPasswordRequest request)
		{
			if (request.NewPassword != request.ConfirmPassword)
				throw new AppException("Password and ConfirmPassword do not match", 400);

			var googleAccount = await _accountRepository.GetByIdWithUserAsync(accountId)
				?? throw new AppException("Account not found", 404);

			if (googleAccount.Provider != AuthProvider.GOOGLE)
				throw new AppException("Only Google account can set local password", 400);

			// kiểm tra đã có local chưa
			var existedLocal = await _accountRepository
				.GetAccountByEmailAndProviderAsync(
					googleAccount.User.Email,
					AuthProvider.LOCAL);

			if (existedLocal != null)
				throw new AppException("Local account already exists", 400);

			var salt = _passwordHasher.GenerateSalt();

			var localAccount = new Account
			{
				Id = Guid.NewGuid(),
				UserId = googleAccount.UserId,
				Provider = AuthProvider.LOCAL,
				Salt = salt,
				PasswordHash = _passwordHasher.Hash(request.NewPassword, salt),
				IsFirstLogin = false,
				CreatedAt = DateTime.UtcNow
			};

			await _accountRepository.AddAsync(localAccount);

			googleAccount.IsFirstLogin = false;
			_accountRepository.Update(googleAccount);

			await _unitOfWork.SaveChangesAsync();

			return new SetLocalPasswordResponse
			{
				Email = googleAccount.User.Email,
				SetAt = DateTime.UtcNow
			};
		}
	}
}
