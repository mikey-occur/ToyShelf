using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Domain.IRepositories
{
	public interface IPasswordResetOtpRepository : IGenericRepository<PasswordResetOtp>
	{
		Task<PasswordResetOtp?> GetWithAccountAsync(string otpCode, OtpPurpose purpose);
	}
}
