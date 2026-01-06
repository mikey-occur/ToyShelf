using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Auth;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Account.Request;
using ToyCabin.Application.Models.Account.Response;
using ToyCabin.Application.Notifications;
using ToyCabin.Application.Security;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class ForgotPasswordService : IForgotPasswordService
	{
		private readonly IAccountRepository _accountRepository;
		private readonly IPasswordHasher _passwordHasher;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPasswordResetOtpRepository _otpRepo;
		private readonly IEmailService _emailService;
		public ForgotPasswordService(IAccountRepository accountRepository,
							  IPasswordHasher passwordHasher,
							  IUnitOfWork unitOfWork,
							  IPasswordResetOtpRepository otpRepo,
							  IEmailService emailService)
		{
			_accountRepository = accountRepository;
			_passwordHasher = passwordHasher;
			_unitOfWork = unitOfWork;
			_otpRepo = otpRepo;
			_emailService = emailService;
		}
		// ================= FLOW FORGETPASSWORD =================
		public async Task<ForgotPasswordOtpResponse> RequestForgotPasswordAsync(string email)
		{
			var account = await _accountRepository
				.GetLocalAccountByEmailAsync(email);

			if (account == null)
				throw new Exception("Account not found");

			// invalidate OTP cũ
			var oldOtps = await _otpRepo.FindAsync(o =>
				o.AccountId == account.Id &&
				!o.IsUsed &&
				o.Purpose == OtpPurpose.RESET_PASSWORD);

			foreach (var o in oldOtps)
				o.IsUsed = true;

			var otpCode = Random.Shared.Next(100000, 999999).ToString();
			var expiredAt = DateTime.UtcNow.AddMinutes(10);

			var otp = new PasswordResetOtp
			{
				Account = account,
				OtpCode = otpCode,
				Purpose = OtpPurpose.RESET_PASSWORD,
				ExpiredAt = expiredAt,
				CreatedAt = DateTime.UtcNow
			};

			await _otpRepo.AddAsync(otp);
			await _unitOfWork.SaveChangesAsync();

			await _emailService.SendOtpAsync(
				toEmail: account.User.Email,
				otpCode: otpCode,
				purpose: OtpPurpose.RESET_PASSWORD,
				expiredAt: expiredAt,
				fullName: account.User.FullName
			);

			return new ForgotPasswordOtpResponse
			{
				Email = account.User.Email,
				ExpiredAt = expiredAt
			};
		}
		public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
		{
			if (request.NewPassword != request.ConfirmPassword)
				throw new Exception("Password and ConfirmPassword do not match");

			var otp = await _otpRepo
				.GetWithAccountAsync(request.OtpCode, OtpPurpose.RESET_PASSWORD);

			if (otp == null)
				throw new Exception("Invalid or expired OTP");

			var account = otp.Account!;

			var salt = _passwordHasher.GenerateSalt();
			account.Salt = salt;
			account.PasswordHash = _passwordHasher.Hash(request.NewPassword, salt);
			account.LastLoginAt = DateTime.UtcNow;

			otp.IsUsed = true;

			_accountRepository.Update(account);
			await _unitOfWork.SaveChangesAsync();

			return new ResetPasswordResponse
			{
				Email = account.User.Email,
				ResetAt = DateTime.UtcNow
			};
		}
	}
}
