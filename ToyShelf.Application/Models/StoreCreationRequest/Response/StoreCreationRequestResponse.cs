using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.StoreCreationRequest.Response
{
	public class StoreCreationRequestResponse
	{
		public Guid Id { get; set; }
		public Guid PartnerId { get; set; }
		public string PartnerName { get; set; } = string.Empty;
		public Guid CityId { get; set; }
		public string CityName { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public string? PhoneNumber { get; set; }
		public StoreRequestStatus Status { get; set; }
		public Guid RequestedByUserId { get; set; }
		public string RequestedByUserName { get; set; } = string.Empty;
		public string RequestedByUserEmail { get; set; } = string.Empty;
		public Guid? ReviewedByUserId { get; set; }
		public string ReviewedByUserName { get; set; } = string.Empty;
		public string ReviewedByUserEmail { get; set; } = string.Empty;
		public string? RejectReason { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ReviewedAt { get; set; }
	}
}
