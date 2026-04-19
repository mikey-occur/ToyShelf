using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.User.Request
{
	public class GetUserByStoreOrPartnerRequest
	{
		public Guid? PartnerId { get; set; }

		public Guid? StoreId { get; set; }

		public StoreRole? StoreRole { get; set; }

		public bool? IsActive { get; set; }
	}
}
