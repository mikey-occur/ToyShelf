using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class WarehouseStatCardResponse
	{
        public Guid WarehouseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Các chỉ số cũ
        public int TotalOrders { get; set; }
        public int TotalShelves { get; set; }
        public int TotalInventory { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalInProgressShipments { get; set; }
        public int TotalCompletedShipments { get; set; }
    }

}
