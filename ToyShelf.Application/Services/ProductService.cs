using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Product.Request;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.ProductCategory.Response;
using ToyShelf.Application.Models.ProductColor.Response;
using ToyShelf.Application.Models.Store.Response;
using ToyShelf.Domain.Common.Product;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _productRepository;
		private readonly IProductCategoryRepository _categoryRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly IProductBroadcaster _productBroadcaster;
		private readonly IProductColorRepository _productColorRepository;
		private readonly IColorRepository _colorRepository;
		public ProductService(IProductRepository productRepository, IProductCategoryRepository categoryRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider,IProductBroadcaster productBroadcaster, IProductColorRepository productColorRepository, IColorRepository colorRepository)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_unitOfWork = unitOfWork;
			_dateTimeProvider = dateTimeProvider;
			_productBroadcaster = productBroadcaster;
			_productColorRepository = productColorRepository;
			_colorRepository = colorRepository;
		}
		//===Create===
		public async Task<ProductResponse> CreateProductAsync(ProductRequest request)
		{
			var category = await _categoryRepository.GetByIdAsync(request.ProductCategoryId);
			if (category == null)
				throw new KeyNotFoundException($"Category not found. Id = {request.ProductCategoryId}");

			// Map category code
			string categoryCode = MapCategoryToCode(category.Code); // robo-dog -> RD

			// Sinh sequence
			int nextSeq = await _productRepository.GetNextSequenceAsync(categoryCode);

			// Build SKU
			string sku = $"{categoryCode}-{nextSeq:D6}";

			var product = new Product
			{
				Name = request.Name,
				ProductCategoryId = request.ProductCategoryId,
				SKU = sku,
				BasePrice = request.Price,
                Description = request.Description,
				Brand = request.Brand,
				Material = request.Material,
				OriginCountry = request.OriginCountry,
				AgeRange = request.AgeRange,
				IsActive = true,
				IsConsignment = true,
				CreatedAt = _dateTimeProvider.UtcNow

			};

			 await _productRepository.AddAsync(product);

			// 4. Xử lý danh sách Colors (nếu có)
			if (request.Colors != null && request.Colors.Any())
			{
				foreach (var colorReq in request.Colors)
				{
					// Lấy thông tin Color để lấy SkuCode (ví dụ: "RED", "BLUE")

					var color = await _colorRepository.GetByIdAsync(colorReq.ColorId)
					?? throw new AppException("Color not found");

					var variantSku = ProductSkuGenerator.GenerateColorComboSku(product.SKU, color.SkuCode);

					var newProductColor = new ProductColor
					{
						Id = Guid.NewGuid(),
						ProductId = product.Id, 
						ColorId = colorReq.ColorId,
						PriceSegmentId = colorReq.PriceSegmentId,
						Price = colorReq.Price,
						Sku = variantSku,
						QrCode = colorReq.QrCode,
						Model3DUrl = colorReq.Model3DUrl,
						ImageUrl = colorReq.ImageUrl,
						IsActive = true
					};

					await _productColorRepository.AddAsync(newProductColor);
				}
			}
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(product);

		}

		//===Delete/Disable===
		public async Task<bool> DeleteProductAsync(Guid id)
		{
			var product =  await _productRepository.GetByIdAsync(id);
			if (product is null)
				throw new Exception($"Product Id = {id} not found");
			_productRepository.Remove(product);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DisableProductAsync(Guid id)
		{
			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
				throw new Exception($"Product Id = {id} not found");
			if (!product.IsActive)
				throw new Exception("Product already inactive");
			product.IsActive = false;
			_productRepository.Update(product);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		//===Get===
	
		public async Task<IEnumerable<ProductResponse>> GetProductsAsync(bool? isActive)
		{
			var products = await  _productRepository.GetProductsAsync(isActive);
			return products.Select(MapToResponse);
		}
		public async Task<ProductResponse> GetByIdAsync(Guid id)
		{
			var product =  await _productRepository.GetByIdAsync(id);
			if (product == null)
				throw new Exception($"Product Id = {id} not found");
			return MapToResponse(product);
		}

		//===Restore===
		public async Task<bool> RestoreProductAsync(Guid id)
		{
			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
				throw new Exception($"Product Id = {id} not found");
			if (product.IsActive)
				throw new Exception("Product already active");
			product.IsActive = true;
			_productRepository.Update(product);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		//===Update===
		public async Task<ProductResponse?> UpdateProductAsync(Guid id, ProductUpdateRequest request)
		{
			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
				throw new Exception($"Product Id = {id} not found");
			// Update fields
			product.Name = request.Name;
			product.BasePrice = request.Price;
			product.Description = request.Description;
			product.Brand = request.Brand;
			product.Material = request.Material;
			product.OriginCountry = request.OriginCountry;
			product.AgeRange = request.AgeRange;
			product.IsConsignment = request.IsConsignment;
			product.UpdatedAt = _dateTimeProvider.UtcNow;
			_productRepository.Update(product);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(product);
		}

		//====Search====
		public async Task<IEnumerable<ProductResponse>> SearchAsync(string keyword, bool? isActive)
		{
			var products = await _productRepository.SearchAsync(keyword, isActive);

			return products.Select(MapToResponse);
		}

		// ===== MAPPER =====
		private ProductResponse MapToResponse(Product product)
		{
			return new ProductResponse
			{
				Id = product.Id,
				ProductCategoryId = product.ProductCategoryId,
				Name = product.Name,
				SKU = product.SKU,
				Description = product.Description,
				Price = product.BasePrice,
				Brand = product.Brand,
				Material = product.Material,
				OriginCountry = product.OriginCountry,
				AgeRange = product.AgeRange,
				IsActive = product.IsActive,
				IsConsignment = product.IsConsignment,
				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt,

				Colors = product.ProductColors
				.Where(c => c.IsActive)
				.Select(c => new ProductColorResponse
				{
					Id = c.Id,
					Sku = c.Sku,
					ColorId = c.ColorId,
					PriceSegmentId = c.PriceSegmentId,
					Price = c.Price,
					QrCode = c.QrCode,
					Model3DUrl = c.Model3DUrl,
					ImageUrl = c.ImageUrl,
					IsActive = c.IsActive
				})
				.ToList()
				};
		}
		//====SKU CODE Convert=====
		public string MapCategoryToCode(string categoryName)
		{
			// Nếu tên category rỗng hoặc null thì trả về chuỗi rỗng
			if (string.IsNullOrWhiteSpace(categoryName))
				return string.Empty;

			// Tách tên category 
			// Ví dụ: "robo-dog" -> ["robo", "dog"]
			var words = categoryName
				.Split(new[] { '-', ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);

			// Lấy chữ cái đầu của mỗi từ và chuyển sang chữ hoa
			// Ví dụ: ["robo", "dog"] -> 'R' + 'D' = "RD"
			var code = string.Concat(
				words.Select(w => char.ToUpperInvariant(w[0]))
			);
			return code;
		}


        public async Task SelectProductAsync(Guid id)
        {
            // 1. Xử lý logic nghiệp vụ (nếu có)
            var product = await _productRepository.GetByIdAsync(id);

            // 2. Gọi thông báo (Service không hề biết SignalR là gì)
            await _productBroadcaster.NotifyProductSelectedAsync(id);
        }

        public async Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetProductsPaginatedAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null, Guid? categoryId = null)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 10;

            var (products, totalCount) = await _productRepository.GetProductsPaginatedAsync(pageNumber, pageSize, isActive,categoryId);

            var productResponses = products.Select(MapToResponse);

            return (productResponses, totalCount);
        }
    }
}
