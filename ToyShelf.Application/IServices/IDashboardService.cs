using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Warehouse.Response;

namespace ToyShelf.Application.IServices
{
	public interface IDashboardService
	{
		Task<WarehouseDashboardResponse> GetWarehouseDashboard(Guid warehouseId);
	}
}
