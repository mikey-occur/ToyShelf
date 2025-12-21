using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;	
		public string FullName { get; set; } = string.Empty;
		public string? AvatarUrl { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
	}
}
