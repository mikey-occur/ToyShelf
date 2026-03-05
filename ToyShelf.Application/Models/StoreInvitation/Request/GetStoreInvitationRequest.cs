using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.StoreInvitation.Request
{
	public class GetStoreInvitationRequest
	{
		public InvitationStatus? Status { get; set; }
		public Guid? StoreId { get; set; }
		public Guid? PartnerId { get; set; }
	}
}
