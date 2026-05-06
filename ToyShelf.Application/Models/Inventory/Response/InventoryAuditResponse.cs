using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Inventory.Response
{
	public class InventoryAuditResponse
	{
		public Guid LocationId { get; set; }
		public Guid ProductColorId { get; set; }

		public int OpeningStock { get; set; }
		public List<InventoryAuditItem> Transactions { get; set; } = new();

		public int ClosingStock { get; set; }
		public int CurrentInventory { get; set; }

		public bool IsMatched { get; set; }
	}

	public class InventoryAuditItem
	{
		public DateTime Date { get; set; }
		public string Type { get; set; } = string.Empty;
		public int Quantity { get; set; } // + / -
		public int BalanceAfter { get; set; }
	}
}
