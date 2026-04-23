using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Product.Request;
using ToyShelf.Application.Models.Product.Response;
using ToyShelf.Application.Models.ProductColor.Request;
using ToyShelf.Application.Models.ProductColor.Response;
using ToyShelf.Application.QRcode;
using ToyShelf.Domain.Common.Product;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
namespace ToyShelf.Application.Services
{
	public class ProductColorService : IProductColorService
	{
        private readonly string _targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private readonly IProductColorRepository _productColorRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly IProductRepository _productRepository;
		private readonly IColorRepository _colorRepository;
		private readonly IQrCodeService _qrCodeService;			
		public ProductColorService(IProductColorRepository productColorRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTime, IProductRepository productRepository, IColorRepository colorRepository, IQrCodeService qrCodeService)
		{
			_productColorRepository = productColorRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_productRepository = productRepository;
			_colorRepository = colorRepository;
			_qrCodeService = qrCodeService;
		}
		//===Create===
		public async Task<ProductColorResponse> CreateProductColorAsync(ProductColorRequest request)
		{
			var product = await _productRepository.GetByIdAsync(request.ProductId) ?? throw new AppException("Product not found");

			var color = await _colorRepository.GetByIdAsync(request.ColorId)
		    ?? throw new AppException("Color not found", 404);

			var sku = ProductSkuGenerator.GenerateColorComboSku(product.SKU,color.SkuCode);

			var skuExists = await _productColorRepository.ExistsBySkuAsync(sku);
			if (skuExists)
				throw new AppException($"ProductColor SKU '{sku}' already exists", 409);
			string qrCode = _qrCodeService.GenerateQrBase64(sku);
			var newProductColor = new ProductColor
			{
				Id = Guid.NewGuid(),
				ProductId = request.ProductId,
				ColorId = request.ColorId,
				Price = request.Price,
				Sku = sku,
				QrCode = qrCode,
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
				throw new AppException($"ProductColor Id = {id} not found", 404);
			_productColorRepository.Remove(productColor);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}
		//===Disable===
		public async Task<bool> DisableProductColorAsync(Guid id)
		{
			var productColor = await _productColorRepository.GetByIdAsync(id);
			if (productColor == null)
				throw new AppException($"ProductColor Id = {id} not found", 404);
			if (!productColor.IsActive)
				throw new AppException("ProductColor already inactive", 404);
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
				throw new AppException($"ProductColor Id = {id} not found", 404);
			if (productColor.IsActive)
				throw new AppException("ProductColor is already active", 404);
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
				throw new AppException($"ProductColor Id = {id} not found", 404);
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
				throw new AppException($"ProductColor Id = {id} not found", 404);
			// Update fields
			productColor.Price = request.Price;
			productColor.QrCode = request.QrCode;
			productColor.ImageUrl = request.ImageUrl;
			_productColorRepository.Update(productColor);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(productColor);
		}

		//===GetByVariantSku===
		public async Task<List<ProductBySkuResponse>> SearchByVariantSkuAsync(string keyword)
		{
			var colors = await _productColorRepository.SearchColorsBySkuAsync(keyword);
			if (colors == null || !colors.Any())
				return new List<ProductBySkuResponse>();

			return colors.Select(color => new ProductBySkuResponse
			{
				ProductId = color.Product.Id,
				ProductColorId = color.Id,
				ProductName = color.Product.Name,
				ProductSku = color.Product.SKU,
				BasePrice = color.Product.BasePrice,
				Price = color.Price,
				Description = color.Product.Description,
				Barcode = color.Product.Barcode,
				QrCode = color.QrCode,
				ImageUrl = color.ImageUrl,
				Brand = color.Product.Brand,
				Material = color.Product.Material,
				OriginCountry = color.Product.OriginCountry,
				AgeRange = color.Product.AgeRange,
				IsConsignment = color.Product.IsConsignment,
				VariantSku = color.Sku,
				ColorName = color.Color.Name
			}).ToList();
		}

        public async Task<bool> UpdateFileProductColorAsync(string sku,bool hasFile)
        {
			var color = _productColorRepository.GetColorBySkuAsync(sku).Result;
			if (color == null)
				throw new AppException($"ProductColor with SKU '{sku}' not found", 404);

			color.HasFile = hasFile;
			 _productColorRepository.Update(color);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        //====Mapper===
        private ProductColorResponse MapToResponse(ProductColor productColor)
		{
			return new ProductColorResponse
			{
				Id = productColor.Id,
				ProductId = productColor.ProductId,
				Sku = productColor.Sku,
				Price = productColor.Price,
				QrCode = productColor.QrCode,
				ImageUrl = productColor.ImageUrl,
				IsActive = productColor.IsActive,
				HasFile = productColor.HasFile
            };
		}

     
    }
}
