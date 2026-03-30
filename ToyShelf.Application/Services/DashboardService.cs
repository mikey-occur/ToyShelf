using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Warehouse.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class DashboardService : IDashboardService
	{
		private readonly IInventoryLocationRepository _inventoryLocationRepository;
		private readonly IShelfRepository _shelfRepository;
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IUserWarehouseRepository _userWarehouseRepository;
		private readonly IStoreOrderRepository _storeOrderRepository;
		private readonly IShipmentRepository _shipmentRepository;

		public DashboardService(
			IInventoryLocationRepository inventoryLocationRepository,
			IShelfRepository shelfRepository,
			IInventoryRepository inventoryRepository,
			IUserWarehouseRepository userWarehouseRepository,
			IStoreOrderRepository storeOrderRepository,
			IShipmentRepository shipmentRepository)
		{
			_inventoryLocationRepository = inventoryLocationRepository;
			_shelfRepository = shelfRepository;
			_inventoryRepository = inventoryRepository;
			_userWarehouseRepository = userWarehouseRepository;
			_storeOrderRepository = storeOrderRepository;
			_shipmentRepository = shipmentRepository;
		}


		public async Task<WarehouseDashboardResponse> GetWarehouseDashboard(Guid warehouseId)
		{
			// ===== STAT CARD =====

			// 1. Total Shelves
			var totalShelves = await _shelfRepository.GetQueryable()
				.CountAsync(x => x.InventoryLocation.WarehouseId == warehouseId);

			// 2. Total Inventory (SUM quantity)
			var totalInventory = await _inventoryRepository.GetQueryable()
				.Where(x => x.InventoryLocation.WarehouseId == warehouseId)
				.SumAsync(x => (int?)x.Quantity) ?? 0;

			// 3. Total Employees
			var totalEmployees = await _userWarehouseRepository.GetQueryable()
				.CountAsync(x => x.WarehouseId == warehouseId);

			// 4. Total Orders (StoreOrder)
			var totalOrders = await _storeOrderRepository.GetQueryable()
				.CountAsync(x => x.StoreLocation.WarehouseId == warehouseId);

			// ===== SHIPMENT =====

			var shipmentQuery = _shipmentRepository.GetQueryable()
				.Where(x => x.FromLocation.WarehouseId == warehouseId);

			// 5. In Progress
			var totalInProgress = await shipmentQuery.CountAsync(x =>
				x.Status == ShipmentStatus.Approved ||
				x.Status == ShipmentStatus.Shipping
			);

			// 6. Completed
			var totalCompleted = await shipmentQuery.CountAsync(x =>
				x.Status == ShipmentStatus.Received
			);

			// 7. Shipment Chart
			var shipmentChart = await shipmentQuery
				.GroupBy(x => x.Status)
				.Select(g => new ChartItem
				{
					Status = g.Key.ToString(),
					Count = g.Count()
				})
				.ToListAsync();

			// ===== STORE ORDER CHART =====

			var orderChart = await _storeOrderRepository.GetQueryable()
				.Where(x => x.StoreLocation.WarehouseId == warehouseId)
				.GroupBy(x => x.Status)
				.Select(g => new ChartItem
				{
					Status = g.Key.ToString(),
					Count = g.Count()
				})
				.ToListAsync();

			return new WarehouseDashboardResponse
			{
				TotalOrders = totalOrders,
				TotalShelves = totalShelves,
				TotalInventory = totalInventory,
				TotalEmployees = totalEmployees,

				TotalInProgressShipments = totalInProgress,
				TotalCompletedShipments = totalCompleted,

				ShipmentChart = shipmentChart,
				OrderChart = orderChart
			};
		}
	}
}
