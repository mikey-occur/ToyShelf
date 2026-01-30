using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Store.Request
{
	public class UpdateStoreRequest
	{
		public string Name { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;
		public string? PhoneNumber { get; set; }
	}
}
