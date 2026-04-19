using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.User.Response
{
	public class UserResponse
	{
		public Guid UserId { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;

		public Guid? PartnerId { get; set; }

		public Guid? StoreId { get; set; }
		public string? StoreName { get; set; }

		public StoreRole? StoreRole { get; set; }
		public bool IsActive { get; set; } = true;
	}

}
