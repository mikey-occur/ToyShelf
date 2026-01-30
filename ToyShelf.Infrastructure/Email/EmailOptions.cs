using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Infrastructure.Email
{
	public class EmailOptions
	{
		public string SmtpHost { get; set; } = null!;
		public int SmtpPort { get; set; }
		public string SenderEmail { get; set; } = null!;
		public string SenderName { get; set; } = null!;
		public string Username { get; set; } = null!;
		public string Password { get; set; } = null!;
		public bool EnableSsl { get; set; }
	}
}
