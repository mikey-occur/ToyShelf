using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public enum InvitationStatus
	{
		Pending, // -> 0
		Accepted, // -> 1
		Rejected, // -> 2
		Expired // -> 3
	}

	public class StoreInvitation
	{
		public Guid Id { get; set; }
		public Guid StoreId { get; set; }
		public Guid UserId { get; set; }
		public Guid InvitedByUserId { get; set; }
		public StoreRole StoreRole { get; set; }
		public InvitationStatus Status { get; set; } // Pending, Accepted, Rejected, Expired
		public DateTime CreatedAt { get; set; }
		public DateTime? ExpiredAt { get; set; }
		public Store Store { get; set; } = null!;
		public User User { get; set; } = null!;
	}

}
