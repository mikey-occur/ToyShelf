using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.StoreCreationRequest.Request
{
	public class ReviewStoreCreationRequest
	{
		public StoreRequestStatus Status { get; set; } 
		public string? RejectReason { get; set; }
	}
}
