using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public enum AuthProvider
	{
		LOCAL,
		GOOGLE
	}
	public class Account
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public AuthProvider Provider { get; set; } = AuthProvider.LOCAL;
		public string? PasswordHash { get; set; }
		public string? Salt { get; set; }
		public bool IsActive { get; set; } = true;
		public bool IsFirstLogin { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public virtual User User { get; set; } = null!;
		public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
		public virtual ICollection<PasswordResetOtp> PasswordResetOtps { get; set; } = new List<PasswordResetOtp>();
	}
}
