using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ShelfOrder.Request
{
	public class CreateShelfOrderRequest
	{
		public string? Note { get; set; }
		public List<CreateShelfOrderItemRequest> Items { get; set; } = new();
	}

	public class CreateShelfOrderItemRequest
	{
		public Guid ShelfTypeId { get; set; }
		public int Quantity { get; set; }
	}
}
