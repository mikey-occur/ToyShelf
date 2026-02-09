using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Color.Request
{
	public class ColorRequest
	{
		public string Name { get; set; } = null!;     
		public string HexCode { get; set; } = null!;  
	}
}
