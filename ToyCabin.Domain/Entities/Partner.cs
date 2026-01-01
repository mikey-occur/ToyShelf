using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class Partner
	{
		public Guid Id { get; set; }
		public string CompanyName { get; set; } = string.Empty;
		public string Tier { get; set; } = "STANDARD";
		public decimal RevenueSharePercent { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual ICollection<User> Users { get; set; } = new List<User>();
		public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
	}
}
