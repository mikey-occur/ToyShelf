using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.UserWarehouse.Response;

namespace ToyShelf.Application.Models.Warehouse.Response
{
	public class WarehouseDetailResponse
	{
		public WarehouseResponse? Warehouse { get; set; }
		public IEnumerable<WarehouseUserResponse>? Users { get; set; }
	}
}

