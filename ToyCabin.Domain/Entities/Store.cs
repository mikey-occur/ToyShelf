using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class Store
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = string.Empty; // STORE-HCM01
		public string Name { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual ICollection<Cabin> Cabins { get; set; } = new List<Cabin>();
	}
}
