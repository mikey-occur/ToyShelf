using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Common
{
	public class ActionResponse
	{
		public bool Success { get; set; }
		public string? Message { get; set; }

		public static ActionResponse Ok(string? message = null)
		{
			return new ActionResponse
			{
				Success = true,
				Message = message
			};
		}
	}
}
