using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.StoreCreationRequest.Request
{
	public class CreateStoreCreationRequest
	{
		public string Name { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public string? PhoneNumber { get; set; }
	}
}
