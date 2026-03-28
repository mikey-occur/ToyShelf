using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ShelfType.Response
{
	public class ShelfTypeResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string? ImageUrl { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public double Depth { get; set; }
		public int TotalLevels { get; set; }
		public string SuitableProductCategoryTypes { get; set; } = string.Empty;
		public string? DisplayGuideline { get; set; }
		public bool IsActive { get; set; }
		public List<ShelfTypeLevelResponse> Levels { get; set; } = new();

		public class ShelfTypeLevelResponse
		{
			public int Level { get; set; }
			public string Name { get; set; } = null!;
			public double ClearanceHeight { get; set; }
			public int RecommendedCapacity { get; set; }
			public string SuitableProductCategoryTypes { get; set; } = string.Empty; 
			public string? DisplayGuideline { get; set; } 
		}
	}
}
