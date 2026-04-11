using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShipmentReceiveViewModel
	{
		public Guid ShipmentId { get; set; }
		public string ShipmentCode { get; set; } = null!;
		public string FromLocationName { get; set; } = null!;
		public string ToLocationName { get; set; } = null!;

		// Danh sách sản phẩm để FE hiển thị form nhập số lượng
		public List<ShipmentProductItemDto> ProductItems { get; set; } = new();

		// Danh sách kệ để FE hiển thị checkbox xác nhận
		public List<ShipmentShelfItemDto> ShelfItems { get; set; } = new();
	}

	public class ShipmentProductItemDto
	{
		public Guid ShipmentItemId { get; set; }
		public Guid ProductColorId { get; set; }
		public string ProductName { get; set; } = null!;
		public string ColorName { get; set; } = null!;
		public string? ImageUrl { get; set; }
		public int ExpectedQuantity { get; set; } // Số lượng tối đa có thể nhận
	}

	public class ShipmentShelfItemDto
	{
		public Guid ShelfShipmentItemId { get; set; }
		public Guid ShelfId { get; set; }
		public string ShelfCode { get; set; } = null!;
		public string ShelfTypeName { get; set; } = null!;
	}
}
