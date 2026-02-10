using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class Color
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;     // Red
		public string HexCode { get; set; } = null!;  // #FF0000
		public string SkuCode { get; set; } = string.Empty; // RD
		public ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
	}
}
