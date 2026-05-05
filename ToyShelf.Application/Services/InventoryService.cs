using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Inventory.Request;
using ToyShelf.Application.Models.Inventory.Response;
using ToyShelf.Application.Models.InventoryTransaction;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class InventoryService : IInventoryService
	{
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IInventoryTransactionRepository _transactionRepository;
		private readonly IProductRepository _productRepository;
		private readonly IUnitOfWork _unitOfWork;

		public InventoryService(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork, IWarehouseRepository warehouseRepository, IInventoryTransactionRepository transactionRepository, IProductRepository productRepository)
		{
			_inventoryRepository = inventoryRepository;
			_unitOfWork = unitOfWork;
			_warehouseRepository = warehouseRepository;
			_transactionRepository = transactionRepository;
			_productRepository = productRepository;
		}
		public async Task<InventoryResponse> RefillAsync(RefillInventoryRequest request)
		{
			if (request.Quantity <= 0)
				throw new AppException("Quantity must be greater than 0", 400);

			var inventory = await _inventoryRepository.GetAsync(
				request.InventoryLocationId,
				request.ProductColorId,
				InventoryStatus.Available);

			if (inventory == null)
			{
				inventory = new Inventory
				{
					Id = Guid.NewGuid(),
					InventoryLocationId = request.InventoryLocationId,
					ProductColorId = request.ProductColorId,
					Status = InventoryStatus.Available,
					Quantity = request.Quantity
				};

				await _inventoryRepository.AddAsync(inventory);
			}
			else
			{
				inventory.Quantity += request.Quantity;
				_inventoryRepository.Update(inventory);
			}

			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(inventory);
		}


		public async Task<IEnumerable<InventoryResponse>> GetInventoriesAsync(
			Guid? locationId,
			InventoryStatus? status)
		{
			var inventories = await _inventoryRepository.GetAllInventoryAsync();

			var query = inventories.AsQueryable();

			if (locationId.HasValue)
			{
				query = query.Where(x => x.InventoryLocationId == locationId.Value);
			}

			if (status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value);
			}

			return query.Select(MapToResponse);
		}


		public async Task<InventoryResponse> GetByIdAsync(Guid id)
		{
			var inventory = await _inventoryRepository.GetByIdAsync(id);

			if (inventory == null)
				throw new AppException("Inventory not found", 404);

			return MapToResponse(inventory);
		}

		public async Task UpdateStockAfterPaymentAsync(Order order)
		{
            foreach (var item in order.OrderItems)
            {
                // 1. Xử lý tồn kho Available (Trừ đi)
                var availableInventory = await _inventoryRepository
                    .GetInventoryAsync(order.StoreId, item.ProductColorId, InventoryStatus.Available);

                if (availableInventory == null)
                    throw new AppException($"Product {item.ProductColorId} not found in store.", 404);

                if (availableInventory.Quantity < item.Quantity)
                    throw new AppException($"Quantity not enough (available: {availableInventory.Quantity}, need: {item.Quantity}).", 404);

                availableInventory.Quantity -= item.Quantity;
                _inventoryRepository.Update(availableInventory);

                // 2. Xử lý tồn kho Sold (Cộng vào)
                var soldInventory = await _inventoryRepository
                    .GetInventoryAsync(order.StoreId, item.ProductColorId, InventoryStatus.Sold);

                if (soldInventory == null)
                {
                    // Nếu cửa hàng này chưa bán sản phẩm này bao giờ -> Tạo mới record Sold
                    soldInventory = new Inventory
                    {
                        Id = Guid.NewGuid(),
                        InventoryLocationId = availableInventory.InventoryLocationId, // Đảm bảo đúng kho
                        ProductColorId = item.ProductColorId,
                        Status = InventoryStatus.Sold,
                        Quantity = item.Quantity
                    };
                    await _inventoryRepository.AddAsync(soldInventory);
                }
                else
                {
                    // Nếu đã từng bán -> Cộng dồn số lượng
                    soldInventory.Quantity += item.Quantity;
                    _inventoryRepository.Update(soldInventory);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

		public async Task<WarehouseInventoryResponse> GetWarehouseInventoryAsync(
			Guid warehouseId,
			int? pageNumber,
			int? pageSize,
			bool? isActive,
			Guid? categoryId,
			string? searchItem)
		{
			// 1. Lấy warehouse
			var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
			if (warehouse == null)
				throw new AppException("Warehouse not found", 404);

			// 2. Lấy inventory
			var inventories = await _inventoryRepository.GetByWarehouseIdAsync(warehouseId);

			// 3. Không có hàng
			if (!inventories.Any())
			{
				return new WarehouseInventoryResponse
				{
					WarehouseId = warehouse.Id,
					WarehouseName = warehouse.Name,
					Products = new List<ProductInventoryItem>(),
					PageNumber = pageNumber,
					PageSize = pageSize,
					TotalCount = 0,
					TotalPages = 0 
				};
			}

			// Filter
			var filtered = inventories
				.Where(i => i.ProductColor != null && i.ProductColor.Product != null);

			if (isActive.HasValue)
				filtered = filtered.Where(i => i.ProductColor.Product.IsActive == isActive.Value);

			if (categoryId.HasValue)
				filtered = filtered.Where(i => i.ProductColor.Product.ProductCategoryId == categoryId.Value);

			if (!string.IsNullOrWhiteSpace(searchItem))
			{
				var keyword = searchItem.Trim().ToLower();

				filtered = filtered.Where(i =>
					i.ProductColor.Product.Name.ToLower().Contains(keyword) ||
					i.ProductColor.Product.SKU.ToLower().Contains(keyword) ||
					(i.ProductColor.Product.Barcode != null &&
					 i.ProductColor.Product.Barcode.ToLower().Contains(keyword))
				);
			}

			var grouped = filtered
				.GroupBy(i => i.ProductColor.ProductId)
				.ToList(); // tránh lỗi lambda

			var totalCount = grouped.Count; // tổng số product trước khi phân trang

			if (pageNumber.HasValue && pageSize.HasValue)
			{
				var skip = (pageNumber.Value - 1) * pageSize.Value;
				grouped = grouped.Skip(skip).Take(pageSize.Value).ToList();
			}

			int? totalPages = null;

			if (pageSize.HasValue && pageSize.Value > 0)
			{
				totalPages = (int)Math.Ceiling((double)totalCount / pageSize.Value);
			}


			var groupedProducts = grouped
				.Select(productGroup =>
				{
					var product = productGroup.First().ProductColor.Product;

					return new ProductInventoryItem
					{
						ProductId = product.Id,
						ProductSKU = product.SKU,
						ProductName = product.Name,
						ProductCategoryId = product.ProductCategoryId,
						ProductCategoryName = product.ProductCategory.Name,
						Description = product.Description,
						Brand = product.Brand,
						Material = product.Material,
						OriginCountry = product.OriginCountry,
						AgeRange = product.AgeRange,
						BasePrice = product.BasePrice,

						Colors = productGroup
							.GroupBy(i => i.ProductColorId)
							.Select(colorGroup => new ColorInventoryItem
							{
								ProductColorId = colorGroup.Key,
								ProductColorSku = colorGroup.First().ProductColor.Sku,
								ColorName = colorGroup.First().ProductColor.Color.Name,
								HexCode = colorGroup.First().ProductColor.Color.HexCode,
								ImageUrl = colorGroup.First().ProductColor.ImageUrl,
								Model3DUrl = colorGroup.First().ProductColor.Model3DUrl,
								ProductColorPrice = colorGroup.First().ProductColor.Price,
								Quantity = colorGroup.Sum(x => x.Quantity)
							})
							.ToList()
					};
				})
				.ToList();

			return new WarehouseInventoryResponse
			{
				WarehouseId = warehouse.Id,
				WarehouseName = warehouse.Name,
				Products = groupedProducts,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = totalPages
			};
		}

		public async Task<LocationInventoryOverviewResponse> GetLocationInventoryOverviewAsync(
				Guid locationId,
				int? pageNumber,
				int? pageSize,
				bool? isActive,
				Guid? categoryId,
				string? searchItem)
		{
			// 1. Lấy location (Warehouse hoặc Store)
			var location = await _inventoryRepository.GetLocationByIdAsync(locationId);
			if (location == null)
				throw new AppException("Location not found", 404);

			// 2. Lấy tất cả inventory trong location này
			var inventories = await _inventoryRepository.GetByLocationAsync(locationId);

			if (!inventories.Any())
			{
				return new LocationInventoryOverviewResponse
				{
					LocationId = location.Id,
					LocationName = location.Name,
					Type = location.Type,
					Products = new List<ProductInventoryOverviewItem>()
				};
			}

			// 3. Filter: loại bỏ inventory không có ProductColor/Product
			var filtered = inventories
				.Where(i => i.ProductColor != null && i.ProductColor.Product != null);

			if (isActive.HasValue)
				filtered = filtered.Where(i => i.ProductColor.Product.IsActive == isActive.Value);

			if (categoryId.HasValue)
				filtered = filtered.Where(i => i.ProductColor.Product.ProductCategoryId == categoryId.Value);

			if (!string.IsNullOrWhiteSpace(searchItem))
			{
				var keyword = searchItem.Trim().ToLower();
				filtered = filtered.Where(i =>
					i.ProductColor.Product.Name.ToLower().Contains(keyword) ||
					i.ProductColor.Product.SKU.ToLower().Contains(keyword) ||
					(i.ProductColor.Product.Barcode != null &&
					 i.ProductColor.Product.Barcode.ToLower().Contains(keyword))
				);
			}

			// 4. Nhóm theo ProductId
			var grouped = filtered
				.GroupBy(i => i.ProductColor.ProductId)
				.ToList(); // tránh lỗi lambda

			var totalCount = grouped.Count; // tổng số product trước khi phân trang

			// 5. Phân trang nếu có
			if (pageNumber.HasValue && pageSize.HasValue)
			{
				var skip = (pageNumber.Value - 1) * pageSize.Value;
				grouped = grouped.Skip(skip).Take(pageSize.Value).ToList();
			}

			int? totalPages = null;

			if (pageSize.HasValue && pageSize.Value > 0)
			{
				totalPages = (int)Math.Ceiling((double)totalCount / pageSize.Value);
			}


			// 6. Map ra ProductInventoryOverviewItem
			var groupedProducts = grouped
				.Select(productGroup =>
				{
					var product = productGroup.First().ProductColor.Product;

					return new ProductInventoryOverviewItem
					{
						ProductId = product.Id,
						ProductSKU = product.SKU,
						ProductName = product.Name,
						ProductCategoryId = product.ProductCategoryId,
						ProductCategoryName = product.ProductCategory.Name,
						Description = product.Description,
						Brand = product.Brand,
						Material = product.Material,
						OriginCountry = product.OriginCountry,
						AgeRange = product.AgeRange,
						BasePrice = product.BasePrice,
						Colors = productGroup
							.Where(i => i.ProductColor.Color != null)
							.GroupBy(i => i.ProductColorId)
							.Select(colorGroup => new ColorInventoryOverviewItem
							{
								ProductColorId = colorGroup.Key,
								ProductColorSku = colorGroup.First().ProductColor.Sku,
								ColorName = colorGroup.First().ProductColor.Color.Name,
								HexCode = colorGroup.First().ProductColor.Color.HexCode,
								ImageUrl = colorGroup.First().ProductColor.ImageUrl,
								Model3DUrl = colorGroup.First().ProductColor.Model3DUrl,
								ProductColorPrice = colorGroup.First().ProductColor.Price,
								Available = colorGroup.Where(x => x.Status == InventoryStatus.Available).Sum(x => x.Quantity),
								InTransit = colorGroup.Where(x => x.Status == InventoryStatus.InTransit).Sum(x => x.Quantity),
								Damaged = colorGroup.Where(x => x.Status == InventoryStatus.Damaged).Sum(x => x.Quantity),
								Sold = colorGroup.Where(x => x.Status == InventoryStatus.Sold).Sum(x => x.Quantity)
							})
							.ToList()
					};
				})
				.ToList();

			return new LocationInventoryOverviewResponse
			{
				LocationId = location.Id,
				LocationName = location.Name,
				Type = location.Type,
				Products = groupedProducts,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount,
				TotalPages = totalPages
			};
		}


		public async Task<List<GlobalInventoryResponse>> GetGlobalInventoryAsync(
				InventoryLocationType? type,
				int? pageNumber,
				int? pageSize,
				bool? isActive,
				Guid? categoryId,
				string? searchItem)
		{
			// 1. Lấy tất cả inventory với chi tiết
			var inventories = await _inventoryRepository.GetAllInventoryWithDetailsAsync(type);
			inventories = inventories.Where(i => i.InventoryLocation != null).ToList();

			// 2. Group theo location
			var groupedByLocation = inventories
				.GroupBy(i => i.InventoryLocationId)
				.ToList();

			var result = new List<GlobalInventoryResponse>();

			foreach (var locationGroup in groupedByLocation)
			{
				var location = locationGroup.First().InventoryLocation!;

				// Filter sản phẩm theo isActive, categoryId, searchItem
				var filteredProducts = locationGroup
					.Where(i => i.ProductColor != null && i.ProductColor.Product != null)
					.Where(i => !isActive.HasValue || i.ProductColor.Product.IsActive == isActive.Value)
					.Where(i => !categoryId.HasValue || i.ProductColor.Product.ProductCategoryId == categoryId.Value)
					.Where(i =>
					{
						if (string.IsNullOrWhiteSpace(searchItem)) return true;
						var keyword = searchItem.Trim().ToLower();
						var product = i.ProductColor.Product;
						return product.Name.ToLower().Contains(keyword)
							|| product.SKU.ToLower().Contains(keyword)
							|| (product.Barcode != null && product.Barcode.ToLower().Contains(keyword));
					})
					.ToList();

				// Tổng số product trước khi phân trang
				var totalCount = filteredProducts.Select(i => i.ProductColor.ProductId).Distinct().Count();

				// Phân trang products
				var pagedProducts = filteredProducts
					.GroupBy(i => i.ProductColor.ProductId)
					.Skip(pageNumber.HasValue && pageSize.HasValue ? (pageNumber.Value - 1) * pageSize.Value : 0)
					.Take(pageSize ?? int.MaxValue)
					.ToList();

				int? totalPages = null;

				if (pageSize.HasValue && pageSize.Value > 0)
				{
					totalPages = (int)Math.Ceiling((double)totalCount / pageSize.Value);
				}


				var productItems = pagedProducts
					.Select(productGroup =>
					{
						var product = productGroup.First().ProductColor.Product!;
						return new GlobalProductInventoryItem
						{
							ProductId = product.Id,
							ProductSKU = product.SKU,
							ProductName = product.Name,
							ProductCategoryId = product.ProductCategoryId,
							ProductCategoryName = product.ProductCategory.Name,
							Description = product.Description,
							Brand = product.Brand,
							Material = product.Material,
							OriginCountry = product.OriginCountry,
							AgeRange = product.AgeRange,
							BasePrice = product.BasePrice,
							Colors = productGroup
								.Where(x => x.ProductColor.Color != null)
								.GroupBy(i => i.ProductColorId)
								.Select(colorGroup =>
								{
									var color = colorGroup.First().ProductColor.Color!;
									return new GlobalColorInventoryItem
									{
										ProductColorId = colorGroup.Key,
										ProductColorSku = colorGroup.First().ProductColor.Sku,
										ColorId = color.Id,
										ColorName = color.Name,
										HexCode = color.HexCode,
										ImageUrl = colorGroup.First().ProductColor.ImageUrl,
										Model3DUrl = colorGroup.First().ProductColor.Model3DUrl,
										ProductColorPrice = colorGroup.First().ProductColor.Price,
										Available = colorGroup.Where(x => x.Status == InventoryStatus.Available).Sum(x => x.Quantity),
										InTransit = colorGroup.Where(x => x.Status == InventoryStatus.InTransit).Sum(x => x.Quantity),
										Damaged = colorGroup.Where(x => x.Status == InventoryStatus.Damaged).Sum(x => x.Quantity),
										Sold = colorGroup.Where(x => x.Status == InventoryStatus.Sold).Sum(x => x.Quantity)
									};
								}).ToList()
						};
					}).ToList();

				result.Add(new GlobalInventoryResponse
				{
					InventoryLocationId = location.Id,
					Id = location.Type == InventoryLocationType.Warehouse
							? location.WarehouseId ?? Guid.Empty
							: location.StoreId ?? Guid.Empty,
					Name = location.Name,
					Type = location.Type,
					Products = productItems,
					PageNumber = pageNumber,
					PageSize = pageSize,
					TotalCount = totalCount,
					TotalPages = totalPages
				});
			}

			return result;
		}



		public async Task<GlobalProductInventoryByProductResponse> GetInventoryByProductAsync(Guid productId)
		{
			var inventories = await _inventoryRepository.GetByProductIdAsync(productId);

			if (!inventories.Any())
				throw new AppException("Product inventory not found", 404);

			var productName = inventories.First().ProductColor?.Product?.Name ?? "Unknown";

			// Nhóm theo location
			var locations = inventories
				.Where(i => i.InventoryLocation != null)
				.GroupBy(i => i.InventoryLocationId)
				.Select(lg =>
				{
					var location = lg.First().InventoryLocation!;

					// Nhóm theo màu trong location
					var colors = lg
						.Where(i => i.ProductColor?.Color != null)
						.GroupBy(i => i.ProductColorId)
						.Select(cg =>
						{
							var color = cg.First().ProductColor!.Color!;
							return new ProductColorInventoryItem
							{
								ColorId = color.Id,
								ColorName = color.Name,
								Available = cg.Where(x => x.Status == InventoryStatus.Available).Sum(x => x.Quantity),
								InTransit = cg.Where(x => x.Status == InventoryStatus.InTransit).Sum(x => x.Quantity),
								Damaged = cg.Where(x => x.Status == InventoryStatus.Damaged).Sum(x => x.Quantity),
								Sold = cg.Where(x => x.Status == InventoryStatus.Sold).Sum(x => x.Quantity)
							};
						})
						.ToList();

					return new LocationInventoryItem
					{
						LocationId = location.Id,
						LocationName = location.Name,
						Type = location.Type.ToString().ToUpper(),
						Colors = colors
					};
				})
				.ToList();

			return new GlobalProductInventoryByProductResponse
			{
				ProductId = productId,
				ProductName = productName,
				Locations = locations
			};
		}

		// Lấy tất cả sản phẩm đang có giao dịch, có filter theo productId, fromLocationId, toLocationId (tùy chọn)
		public async Task<IEnumerable<InventoryTransactionResponse>> GetAllTransactionsAsync(
			Guid? productId = null,
			Guid? fromLocationId = null,
			Guid? toLocationId = null)
		{
			// Dùng repository chuẩn, đã filter theo Id
			var transactions = await _transactionRepository.GetAllTransactionsAsync(
				productId,
				fromLocationId,
				toLocationId
			);

			// Map sang response
			return transactions.Select(t => new InventoryTransactionResponse
			{
				TransactionId = t.Id,
				ProductColorId = t.ProductColorId,
				ColorName = t.ProductColor.Color?.Name ?? "Unknown",
				ProductName = t.ProductColor.Product.Name,
				FromLocation = t.FromLocation?.Name ?? "N/A",
				ToLocation = t.ToLocation?.Name ?? "N/A",
				Quantity = t.Quantity,
				CreatedAt = t.CreatedAt
			}).ToList();
		}

		private static InventoryResponse MapToResponse(Inventory inventory)
		{
			return new InventoryResponse
			{
				Id = inventory.Id,
				InventoryLocationId = inventory.InventoryLocationId,
				ProductColorId = inventory.ProductColorId,
				Status = inventory.Status,
				Quantity = inventory.Quantity
			};
		}
	}
}
