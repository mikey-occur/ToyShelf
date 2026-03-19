using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.StoreOrder.Request
{
	public class CreateStoreOrderRequest
	{
		public List<CreateStoreOrderItemRequest> Items { get; set; } = new();
	}

	public class CreateStoreOrderItemRequest
	{
		public Guid ProductColorId { get; set; }
		public int Quantity { get; set; }
	}

}
