using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.InventoryTransaction
{
	public class InventoryTransactionResponse
	{
		public Guid TransactionId { get; set; }
		public Guid ProductColorId { get; set; }
		public string ColorName { get; set; } = null!;
		public string ProductName { get; set; } = null!;
		public string FromLocation { get; set; } = "N/A";
		public string ToLocation { get; set; } = "N/A";
		public int Quantity { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
