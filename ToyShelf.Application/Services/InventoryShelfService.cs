using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.InventoryShelf.Response;
using ToyShelf.Application.Models.Shelf.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class InventoryShelfService : IInventoryShelfService
	{
		private readonly IInventoryShelfRepository _shelfRepo;
		private readonly IStoreRepository _storeRepo;
		private readonly IInventoryLocationRepository _locationRepo;
		private readonly IUnitOfWork _unitOfWork;

		// Nhúng toàn bộ qua Interface, không dính dáng gì tới DbContext
		public InventoryShelfService(
			IInventoryShelfRepository shelfRepo,
			IStoreRepository storeRepo,
			IInventoryLocationRepository locationRepo,
			IUnitOfWork unitOfWork)
		{
			_shelfRepo = shelfRepo;
			_storeRepo = storeRepo;
			_locationRepo = locationRepo;
			_unitOfWork = unitOfWork;
		}

		public async Task<List<InventoryShelfResponse>> GetShelvesByLocationAsync(Guid locationId)
		{
			var shelves = await _shelfRepo.GetShelvesByLocationAsync(locationId);
			return shelves.Select(s => new InventoryShelfResponse
			{
				InventoryLocationId = s.InventoryLocationId,
				LocationName = s.InventoryLocation.Name,
				ShelfTypeId = s.ShelfTypeId,
				ShelfTypeName = s.ShelfType.Name,
				ImageUrl = s.ShelfType.ImageUrl,
				Width = s.ShelfType.Width,
				Height = s.ShelfType.Height,
				Depth = s.ShelfType.Depth,
				DisplayGuideline = s.ShelfType.DisplayGuideline,
				Quantity = s.Quantity,
				TotalLevels = s.ShelfType.TotalLevels,
				Status = s.Status
			}).ToList();
		}

		// goi ham nay de add quantity vao ke, neu ke chua co thi tao moi, sau do update so luong va luu vao db
		public async Task AddShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity)
		{
			if (quantity <= 0) throw new ArgumentException("Số lượng nhập phải lớn hơn 0");

			int maxRetries = 3;
			for (int retry = 0; retry < maxRetries; retry++)
			{
				await _unitOfWork.BeginTransactionAsync(); 
				try
				{
					var location = await _locationRepo.GetByIdAsync(locationId);
					if (location == null) throw new AppException("Không tìm thấy kho!", 404);

					var shelf = await _shelfRepo.GetShelfAsync(locationId, shelfTypeId);
					if (shelf == null)
					{
						shelf = new InventoryShelf(locationId, shelfTypeId, 0, ShelfStatus.Available);
						await _shelfRepo.AddAsync(shelf);
					}

					shelf.AddQuantity(quantity);

					if (location.Type == InventoryLocationType.Store && location.StoreId.HasValue)
					{
						var store = await _storeRepo.GetByIdAsync(location.StoreId.Value);
						if (store != null)
						{
							store.IncreaseShelfCount(quantity);
						}
					}

					await _unitOfWork.SaveChangesAsync();
					await _unitOfWork.CommitTransactionAsync();
					return;
				}
				catch (DbUpdateConcurrencyException ex)
				{
					await _unitOfWork.RollbackTransactionAsync();

					if (retry == maxRetries - 1)
						throw new AppException("Hệ thống đang có nhiều người thao tác cùng lúc, vui lòng thử lại sau!", 409);

					foreach (var entry in ex.Entries)
					{
						entry.State = EntityState.Detached;
					}
					await Task.Delay(100);
				}
				catch (Exception)
				{
					await _unitOfWork.RollbackTransactionAsync();
					throw;
				}
			}
		}
		// goi ham nay de remove quantity khoi ke, neu ke khong co hoac so luong khong du thi bao loi, sau do update so luong va luu vao db
		public async Task RemoveShelfQuantityAsync(Guid locationId, Guid shelfTypeId, int quantity)
		{
			if (quantity <= 0) throw new ArgumentException("Số lượng xuất phải lớn hơn 0");

			int maxRetries = 3;
			for (int retry = 0; retry < maxRetries; retry++)
			{
				await _unitOfWork.BeginTransactionAsync();
				try
				{
					var location = await _locationRepo.GetByIdAsync(locationId);
					if (location == null) throw new AppException("Không tìm thấy kho!", 404);

					var shelf = await _shelfRepo.GetShelfAsync(locationId, shelfTypeId);
					if (shelf == null) throw new AppException("Kệ này không tồn tại trong kho!", 404);

					shelf.RemoveQuantity(quantity);

					if (location.Type == InventoryLocationType.Store && location.StoreId.HasValue)
					{
						var store = await _storeRepo.GetByIdAsync(location.StoreId.Value);
						if (store != null)
						{
							store.DecreaseShelfCount(quantity);
						}
					}

					await _unitOfWork.SaveChangesAsync();
					await _unitOfWork.CommitTransactionAsync();
					return;
				}
				catch (DbUpdateConcurrencyException ex)
				{
					await _unitOfWork.RollbackTransactionAsync();
					if (retry == maxRetries - 1) throw new AppException("Hệ thống bận, vui lòng thử lại sau!", 409);

					foreach (var entry in ex.Entries) entry.State = EntityState.Detached;
					await Task.Delay(100);
				}
				catch (Exception)
				{
					await _unitOfWork.RollbackTransactionAsync();
					throw;
				}
			}
		}

		public async Task<List<ShelfDistributionResponse>> GetShelfDistributionsAsync(Guid shelfTypeId)
		{
			var data = await _shelfRepo.GetDistributionsByShelfTypeAsync(shelfTypeId);

			return data.Select(x => new ShelfDistributionResponse
			{
				InventoryLocationId = x.InventoryLocationId,
				InventoryLocationName = x.InventoryLocation?.Name ?? "N/A",
				Quantity = x.Quantity,
				Shelf = new ShelfDetailResponse
				{
					ShelfTypeId = x.ShelfTypeId,
					ShelfTypeName = x.ShelfType?.Name ?? "N/A",
					Width = x.ShelfType?.Width ?? 0,
					Height = x.ShelfType?.Height ?? 0,
					Depth = x.ShelfType?.Depth ?? 0,
					ImageUrl = x.ShelfType?.ImageUrl,
					DisplayGuideline = x.ShelfType?.DisplayGuideline
				}
			}).ToList();
		}
	}
}
