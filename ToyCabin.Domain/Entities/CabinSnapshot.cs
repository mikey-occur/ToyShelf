using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class CabinSnapshot
	{
		public Guid Id { get; set; }

		public Guid CabinId { get; set; }
		public Guid UserId { get; set; }

		public string EventType { get; set; } = null!;   // OPEN_DOOR, REFILL, CUSTOMER_INTERACT, ERROR
		public string ImageUrl { get; set; } = null!;
		public DateTime TakenAt { get; set; }

		// Navigation
		public virtual Cabin Cabin { get; set; } = null!;
		public virtual User User { get; set; } = null!;
	}

}
