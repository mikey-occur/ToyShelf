using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Payment
{
	public class PayOSOptions
	{
		public string BaseUrl { get; set; } = string.Empty;
		public string ClientId { get; set; } = string.Empty;
		public string ApiKey { get; set; } = string.Empty;
		public string ChecksumKey { get; set; } = string.Empty;
		public string WebhookUrl { get; set; } = string.Empty;
	}
}
