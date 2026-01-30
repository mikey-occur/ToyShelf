using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Common;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Product.Request;
using ToyCabin.Application.Models.Product.Response;
using ToyCabin.Application.Models.ProductColor.Request;
using ToyCabin.Application.Models.ProductColor.Response;
using ToyCabin.Domain.Common.Product;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;
namespace ToyCabin.Application.Services
{
	public class ProductColorService : IProductColorService
	{

		private readonly IProductColorRepository _productColorRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly IProductRepository _productRepository;
		public ProductColorService(IProductColorRepository productColorRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTime, IProductRepository productRepository)
		{
			_productColorRepository = productColorRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_productRepository = productRepository;
		}
		//===Create===
		public async Task<ProductColorResponse> CreateProductColorAsync(ProductColorRequest request)
		{
			var product = await _productRepository.GetByIdAsync(request.ProductId) ?? throw new AppException("Product not found");

			var sku = ProductSkuGenerator.GenerateColorComboSku(product.SKU,request.Name);

			var skuExists = await _productColorRepository.ExistsBySkuAsync(sku);
			if (skuExists)
				throw new AppException($"ProductColor SKU '{sku}' already exists");
			var newProductColor = new ProductColor
			{
				Id = Guid.NewGuid(),
				ProductId = request.ProductId,
				Sku = sku,
				Name = request.Name,
				HexCode = request.HexCode,
				QrCode = request.QrCode,
				Model3DUrl = request.Model3DUrl,
				ImageUrl = request.ImageUrl,
				IsActive = true
			};
			await _productColorRepository.AddAsync(newProductColor);
			await _unitOfWork.SaveChangesAsync();

		    return MapToResponse(newProductColor);
		}
		//===Delete===
		public async Task<bool> DeleteProductColorAsync(Guid id)
		{
			var productColor = await  _productColorRepository.GetByIdAsync(id);
			if (productColor is null)
				throw new Exception($"ProductColor Id = {id} not found");
			_productColorRepository.Remove(productColor);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}
		//===Disable===
		public async Task<bool> DisableProductColorAsync(Guid id)
		{
			var productColor = await _productColorRepository.GetByIdAsync(id);
			if (productColor == null)
				throw new Exception($"ProductColor Id = {id} not found");
			if (!productColor.IsActive)
				throw new Exception("ProductColor already inactive");
			productColor.IsActive = false;
			_productColorRepository.Update(productColor);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}
		//===Restore===
		public async Task<bool> RestoreProductColorAsync(Guid id)
		{
			var productColor = await _productColorRepository.GetByIdAsync(id);
			if (productColor == null)
				throw new Exception($"ProductColor Id = {id} not found");
			if (productColor.IsActive)
				throw new Exception("ProductColor is already active");
			productColor.IsActive = true;
			_productColorRepository.Update(productColor);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		//===GetById===
		public async Task<ProductColorResponse> GetByIdAsync(Guid id)
		{
			var productColor = await _productColorRepository.GetByIdAsync(id);
			if (productColor == null)
				throw new Exception($"ProductColor Id = {id} not found");
			return MapToResponse(productColor);
		}

		//===GetAll/Active/Inactive/ByStatus===
		public async Task<IEnumerable<ProductColorResponse>> GetProductColorsAsync(bool? isActive)
		{
			var productColors = await _productColorRepository.GetProductColorsAsync(isActive);
			return productColors.Select(MapToResponse);
		}

		//===Update===
		public async Task<ProductColorResponse?> UpdateProductColorAsync(Guid id, ProductColorUpdateRequest request)
		{
			var productColor = await _productColorRepository.GetByIdAsync(id);
			if (productColor == null)
				throw new Exception($"ProductColor Id = {id} not found");
			// Update fields
			productColor.Name = request.Name;
			productColor.HexCode = request.HexCode;
			productColor.QrCode = request.QrCode;
			productColor.Model3DUrl = request.Model3DUrl;
			productColor.ImageUrl = request.ImageUrl;
			_productColorRepository.Update(productColor);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(productColor);
		}

		//===GetByVariantSku===
		public async Task<ProductBySkuResponse?> GetByVariantSkuAsync(string sku)
		{
			var color = await _productColorRepository.GetColorBySkuAsync(sku);

			if (color == null)
				return null;

			return new ProductBySkuResponse
			{
				ProductId = color.Product.Id,
				ProductName = color.Product.Name,
				ProductSku = color.Product.SKU,
				Price = color.Product.BasePrice,
				Description = color.Product.Description,
				QrCode = color.QrCode,
				Model3DUrl = color.Model3DUrl,
				ImageUrl = color.ImageUrl,
				Brand = color.Product.Brand,
				Material = color.Product.Material,
				OriginCountry = color.Product.OriginCountry,
				AgeRange = color.Product.AgeRange,
				IsConsignment = color.Product.IsConsignment,
				VariantSku = color.Sku,
				ColorName = color.Name
			};
		}

	
		//====Mapper===
		private ProductColorResponse MapToResponse(ProductColor productColor)
		{
			return new ProductColorResponse
			{
				Id = productColor.Id,
				Name = productColor.Name,
				ProductId = productColor.ProductId,
				Sku = productColor.Sku,
				HexCode = productColor.HexCode,
				QrCode = productColor.QrCode,
				Model3DUrl = productColor.Model3DUrl,
				ImageUrl = productColor.ImageUrl,
				IsActive = productColor.IsActive
			};
		}


	}
}
