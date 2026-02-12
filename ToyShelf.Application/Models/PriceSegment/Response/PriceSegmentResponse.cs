using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.PriceSegment.Response
{
	public class PriceSegmentResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public decimal MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }
	}
}
