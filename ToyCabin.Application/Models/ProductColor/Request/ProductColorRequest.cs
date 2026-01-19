using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Models.ProductColor.Request
{
	public class ProductColorRequest
	{
		public Guid ProductId { get; set; }
		public string Name { get; set; } = null!;
		public string HexCode { get; set; } = null!;
	}
}
