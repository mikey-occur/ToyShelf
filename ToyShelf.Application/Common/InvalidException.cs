using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Common
{
	public class InvalidException : Exception
	{
		public int StatusCode { get; }
		public string? ErrorCode { get; }
		public InvalidException(string message, int statusCode = 400, string? errorCode = null)
			: base(message)
		{
			StatusCode = statusCode;
			ErrorCode = errorCode;
		}
	}
}
