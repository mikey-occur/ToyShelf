using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public enum OtpPurpose
	{
		ACTIVATE_ACCOUNT,
		RESET_PASSWORD
	}
	public class PasswordResetOtp
	{
		public Guid Id { get; set; }
		public Guid AccountId { get; set; }
		public string OtpCode { get; set; } = null!;
		public OtpPurpose Purpose { get; set; } = OtpPurpose.ACTIVATE_ACCOUNT;
		public bool IsUsed { get; set; } = false;
		public DateTime ExpiredAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public virtual Account Account { get; set; } = null!;
	}
}
