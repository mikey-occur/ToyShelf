using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class InventoryShelf
	{
		public Guid InventoryLocationId { get; private set; }
        public Guid ShelfTypeId { get; private set; }
        public int Quantity { get; private set; }

		// Chống xung đột dữ liệu cho bảng tồn kho chi tiết
		public int RowVersion { get; private set; } = 1;

		public virtual InventoryLocation InventoryLocation { get; private set; } = null!;
        public virtual ShelfType ShelfType { get; private set; } = null!;

        private InventoryShelf() { }

        public InventoryShelf(Guid locationId, Guid shelfTypeId, int initialQuantity)
        {
            InventoryLocationId = locationId;
            ShelfTypeId = shelfTypeId;
            Quantity = initialQuantity >= 0 ? initialQuantity : 0;
        }

        // Thêm nhiều cái một lúc
        public void AddQuantity(int count)
        {
            if (count <= 0) return; // Không làm gì nếu count = 0
            Quantity += count;
			RowVersion++;
		}

        // Trừ nhiều cái một lúc
        public void RemoveQuantity(int count)
        {
            if (count <= 0) return;
            if (Quantity < count) 
                throw new InvalidOperationException($"Tồn kho không đủ. Hiện có {Quantity}, yêu cầu trừ {count}.");
            
            Quantity -= count;
			RowVersion++;
		}

        // Phương thức hỗ trợ kiểm tra nhanh (Dùng ở Application Layer)
        public bool HasEnoughStock(int requestedCount) => Quantity >= requestedCount;
	}
}
