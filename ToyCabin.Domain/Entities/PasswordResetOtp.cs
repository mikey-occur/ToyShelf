using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class PasswordResetOtp
	{
		public Guid OtpId { get; set; }
		public Guid AccountId { get; set; }
		public string OtpCode { get; set; } = null!;
		public bool? IsUsed { get; set; }
		public DateTime ExpiredAt { get; set; }
		public DateTime? CreatedAt { get; set; }
		public virtual Account Account { get; set; } = null!;
	}
}
