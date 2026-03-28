using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class ShelfTypeLevel
	{
		public Guid Id { get; set; }
		public Guid ShelfTypeId { get; set; }

		// số Tầng của kệ
		public int Level { get; set; }

		public string Name { get; set; } = string.Empty; 

		// --- THÔNG SỐ VẬT LÝ CỦA từng tầng ---
		public double ClearanceHeight { get; set; } // Độ cao của tầng
		public double MaxWeightCapacity { get; set; } // Tải trọng của riêng tầng này

		public int RecommendedCapacity { get; set; } // Tầng này nhét bao nhiêu hộp là đẹp nhất
		public string SuitableProductCategoryTypes { get; set; } = string.Empty; // trưng loại sản phẩm nào là đẹp
		public string? DisplayGuideline { get; set; } // Hướng dẫn trưng bày riêng cho tầng này (nếu có)

		public virtual ShelfType ShelfType { get; set; } = null!;
	}
}
