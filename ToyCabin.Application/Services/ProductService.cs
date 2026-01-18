using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.ProductCategory.Response;
using ToyCabin.Application.Models.Store.Response;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _productRepository;
		private readonly IProductCategoryRepository _categoryRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTimeProvider;
		public ProductService(IProductRepository productRepository, IProductCategoryRepository categoryRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_unitOfWork = unitOfWork;
			_dateTimeProvider = dateTimeProvider;
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
				QrCode = request.QrCode,
				Model3DUrl = request.Model3DUrl,
				ImageUrl = request.ImageUrl,
				Brand = request.Brand,
				Material = request.Material,
				OriginCountry = request.OriginCountry,
				AgeRange = request.AgeRange,
				IsActive = true,
				IsConsignment = true,
				CreatedAt = _dateTimeProvider.UtcNow

			};

			 await _productRepository.AddAsync(product);
             await _unitOfWork.SaveChangesAsync();
			 return MapToResponse(product);

		}

		//===Delete/Disable===
		public async Task<bool> DeleteProductAsync(Guid id)
		{
			var product =  _productRepository.GetByIdAsync(id);
			if (product == null)
				throw new Exception($"Product Id = {id} not found");
			_productRepository.Remove(product.Result);
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
		public async Task<IEnumerable<ProductResponse>> GetActiveProductsAsync()
		{
			var products = await  _productRepository.FindAsync(p => p.IsActive);
			return products.Select(p => MapToResponse(p));

		}

		public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync()
		{
			var products = await _productRepository.GetAllAsync();
			return products.Select(MapToResponse);
		}

		public async Task<IEnumerable<ProductResponse>> GetInactiveProductsAsync()
		{
			var products = await  _productRepository.FindAsync(p => !p.IsActive);
			return products.Select(MapToResponse);
		}


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
			product.QrCode = request.QrCode;
			product.Model3DUrl = request.Model3DUrl;
			product.ImageUrl = request.ImageUrl;
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
				QrCode = product.QrCode,
				Model3DUrl = product.Model3DUrl				,
				ImageUrl = product.ImageUrl,
				Brand = product.Brand,
				Material = product.Material,
				OriginCountry = product.OriginCountry,
				AgeRange = product.AgeRange,
				IsActive = product.IsActive,
				IsConsignment = product.IsConsignment,
				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt
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

		
	}
}
