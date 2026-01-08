using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;
using ToyCabin.Infrastructure.Context;

namespace ToyCabin.Infrastructure.Repositories
{
	public class PasswordResetOtpRepository : GenericRepository<PasswordResetOtp>, IPasswordResetOtpRepository
	{
		public PasswordResetOtpRepository(ToyCabinDbContext context) : base(context){}
		public async Task<PasswordResetOtp?> GetWithAccountAsync(string otpCode, OtpPurpose purpose, string email)
		{
			return await _context.PasswordResetOtps
				.Include(o => o.Account)      
					.ThenInclude(a => a.User)    
				.FirstOrDefaultAsync(o =>
					o.OtpCode == otpCode &&
					o.Purpose == purpose &&
					!o.IsUsed &&
					o.ExpiredAt > DateTime.UtcNow &&
					o.Account.User.Email == email);
		}
	}
}
