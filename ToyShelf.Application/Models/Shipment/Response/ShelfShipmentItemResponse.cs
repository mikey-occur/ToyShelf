using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShelfShipmentItemResponse
	{
		public Guid ShelfId { get; set; }
		public string Code { get; set; } = null!;
		public string ShelfTypeName { get; set; } = null!;
		public ShelfShipmentStatus Status { get; set; }
	}
}
