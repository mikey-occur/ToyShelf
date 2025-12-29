using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.ProductCategory.Request
{
	public class ProductCategoryRequest
	{
		public string Code { get; set; } = string.Empty; 
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
	}
}
