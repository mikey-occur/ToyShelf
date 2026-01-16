using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class InventoryLocation
	{
		public Guid Id { get; set; }
		public string Type { get; set; } = null!; // WAREHOUSE, STORE
		public Guid? WarehouseId { get; set; }
		public Guid? StoreId { get; set; }
		public string Name { get; set; } = null!;
		public bool IsActive { get; set; }

		public virtual Warehouse Warehouse { get; set; } = null!;
		public virtual Store Store { get; set; } = null!;

		public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
		public virtual ICollection<Shipment> FromShipments { get; set; } = new List<Shipment>();
		public virtual ICollection<Shipment> ToShipments { get; set; } = new List<Shipment>();
		public virtual ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();
		public virtual ICollection<InventoryTransaction> OutgoingInventoryTransactions { get; set; } = new List<InventoryTransaction>();
		public virtual ICollection<InventoryTransaction> IncomingInventoryTransactions { get; set; } = new List<InventoryTransaction>();
	}

}
