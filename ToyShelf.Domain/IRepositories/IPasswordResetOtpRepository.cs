using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IPasswordResetOtpRepository : IGenericRepository<PasswordResetOtp>
	{
		Task<PasswordResetOtp?> GetWithAccountAsync(string otpCode, OtpPurpose purpose, string email);
	}
}
