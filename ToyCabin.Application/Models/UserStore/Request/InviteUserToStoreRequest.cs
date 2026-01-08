using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Application.Models.UserStore.Request
{
	public class InviteUserToStoreRequest
	{
		public Guid StoreId { get; set; }
		public string Email { get; set; } = string.Empty;
		public StoreRole StoreRole { get; set; }
	}
}
