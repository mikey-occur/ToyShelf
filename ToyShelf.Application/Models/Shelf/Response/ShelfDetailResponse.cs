using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shelf.Response
{
	public class ShelfDetailResponse
	{
		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = null!;
		public double Width { get; set; }
		public double Height { get; set; }
		public double Depth { get; set; }
		public string? ImageUrl { get; set; }
		public string? DisplayGuideline { get; set; }
	}
}
