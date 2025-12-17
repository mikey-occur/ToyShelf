using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class Account
	{
		public Guid Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string Salt { get; set; } = string.Empty;
		public string? AvatarUrl { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
		public virtual ICollection<PasswordResetOtp> PasswordResetOtps { get; set; } = new List<PasswordResetOtp>();
	}
}
