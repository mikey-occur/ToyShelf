using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.PriceSegment.Request
{
	public class PriceSegmentUpdateRequest
	{
		public string Name { get; set; } = null!;
		public decimal MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
	}
}
