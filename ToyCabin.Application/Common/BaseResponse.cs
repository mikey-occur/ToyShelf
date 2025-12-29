using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Models.ProductCategory.Response;

namespace ToyCabin.Application.Common
{
	public class BaseResponse<T>
	{
		public bool Success { get; set; }
		public string? Message { get; set; }
		public T? Data { get; set; }

		public static BaseResponse<T> Ok(T data, string? message = null)
		{
			return new BaseResponse<T>
			{
				Success = true,
				Message = message,
				Data = data
			};
		}

	}
}
