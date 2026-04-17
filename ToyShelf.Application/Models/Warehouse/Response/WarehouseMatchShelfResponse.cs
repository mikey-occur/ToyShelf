using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Warehouse.Response
{
	public class WarehouseMatchShelfResponse
	{
		public Guid WarehouseId { get; set; }
		public Guid WarehouseLocationId { get; set; }
		public string WarehouseName { get; set; } = null!;
		public string WarehouseCode { get; set; } = null!;

		public List<WarehouseShelfItemResponse> Items { get; set; } = new();
	}

	public class WarehouseShelfItemResponse
	{
		public Guid ShelfOrderItemId { get; set; }
		public Guid ShelfTypeId { get; set; }
		public string ShelfTypeName { get; set; } = null!;
		public string? ImageUrl { get; set; } 
		public int AvailableQuantity { get; set; }
		public int OriginalQuantity { get; set; }  // Số lượng yêu cầu ban đầu
		public int FulfilledQuantity { get; set; } // Đã giao bao nhiêu
		public int RemainingQuantity { get; set; }  // Cần thêm bao nhiêu
	}
}
