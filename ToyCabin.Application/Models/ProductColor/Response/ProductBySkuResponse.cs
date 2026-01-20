using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.ProductColor.Response
{
	public class ProductBySkuResponse
	{
		public Guid ProductId { get; set; }
		public string ProductSku { get; set; } = null!;
		public string ProductName { get; set; } = null!;
		public string VariantSku { get; set; } = null!;
		public string ColorName { get; set; } = null!;

	}
}
