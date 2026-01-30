using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ProductCategory.Request
{
	public class UpdateProductCategoryRequest
	{
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
	}
}
