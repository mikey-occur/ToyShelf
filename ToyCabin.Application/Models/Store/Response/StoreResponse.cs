using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.Store.Response
{
	public class StoreResponse
	{
		public Guid Id { get; set; }
		public Guid PartnerId { get; set; }
		public string Code { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
