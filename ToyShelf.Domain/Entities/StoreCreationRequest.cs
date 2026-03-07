using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public enum StoreRequestStatus
	{
		Pending = 0,
		Approved = 1,
		Rejected = 2
	}

	public class StoreCreationRequest
	{
		public Guid Id { get; set; }

		public Guid PartnerId { get; set; }

		public Guid RequestedByUserId { get; set; }

		public Guid? ReviewedByUserId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string StoreAddress { get; set; } = string.Empty;

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }

		public string? PhoneNumber { get; set; }

		public StoreRequestStatus Status { get; set; } = StoreRequestStatus.Pending;

		public string? RejectReason { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime? ReviewedAt { get; set; }


		public virtual Partner Partner { get; set; } = null!;

		public virtual User RequestedByUser { get; set; } = null!;

		public virtual User? ReviewedByUser { get; set; }
	}
}
