using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.StoreInvitation.Response
{
	public class StoreInvitationResponse
	{
		public Guid Id { get; set; }
		public Guid StoreId { get; set; }
		public Guid UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string StoreName { get; set; } = string.Empty;	
		public StoreRole StoreRole { get; set; }
		public InvitationStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ExpiredAt { get; set; }
	}
}
