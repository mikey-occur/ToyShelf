using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.ProductCategory.Response;
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
				Price = request.Price,
                Description = request.Description,
				QrCode = request.QrCode,
				Model3DUrl = request.Model3DUrl,
				ImageUrl = request.ImageUrl,
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
			product.Price = request.Price;
			product.Description = request.Description;
			product.QrCode = request.QrCode;
			product.Model3DUrl = request.Model3DUrl;
			product.ImageUrl = request.ImageUrl;
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
				Name = product.Name,
				SKU = product.SKU,
				Description = product.Description,
				Price = product.Price,
				IsActive = product.IsActive,
				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt
			};
		}
		//====SKU CODE Conver=====
		public string MapCategoryToCode(string categoryName)
		{
			// Xóa dấu, space, dấu -, giữ chữ cái đầu viết hoa
			var code = new string(categoryName
				.Where(c => char.IsLetterOrDigit(c))
				.ToArray())
				.ToUpper();

			// Lấy tối đa 4–5 ký tự
			return code.Length > 4 ? code.Substring(0, 4) : code;
		}

	}
}
