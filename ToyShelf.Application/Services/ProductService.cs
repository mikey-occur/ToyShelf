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
using ToyShelf.Application.QRcode;
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
		private readonly IQrCodeService _qrCodeService;
		private readonly IProductColorRepository _productColorRepository;
		private readonly IColorRepository _colorRepository;
		public ProductService(IProductRepository productRepository, IProductCategoryRepository categoryRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider,IProductBroadcaster productBroadcaster, IProductColorRepository productColorRepository, IColorRepository colorRepository, IQrCodeService qrCodeService)
		{
			_productRepository = productRepository;
			_categoryRepository = categoryRepository;
			_unitOfWork = unitOfWork;
			_dateTimeProvider = dateTimeProvider;
			_productBroadcaster = productBroadcaster;
			_productColorRepository = productColorRepository;
			_colorRepository = colorRepository;
			_qrCodeService = qrCodeService;
		}
		//===Create===
		public async Task<ProductResponse> CreateProductAsync(ProductRequest request)
		{
			var category = await _categoryRepository.GetByIdAsync(request.ProductCategoryId);
			if (category == null)
				throw new KeyNotFoundException($"Category not found. Id = {request.ProductCategoryId}");
			if (!string.IsNullOrWhiteSpace(request.Barcode))
			{
				bool isDuplicate = await _productRepository.IsBarcodeExistsAsync(request.Barcode);
				if (isDuplicate)
				{
					throw new Exception($"Mã Barcode '{request.Barcode}' đã tồn tại trong hệ thống ToyShelf.");
				}
			}
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
				BasePrice = request.BasePrice,
                Description = request.Description,
				Barcode = request.Barcode?.Trim(),
				Brand = request.Brand,
				Material = request.Material,
				OriginCountry = request.OriginCountry,
				AgeRange = request.AgeRange,
				Weight = request.Weight,
				Length = request.Length,
				Height = request.Height,
				Width = request.Width,
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
					?? throw new AppException("Color not found", 404);

					var variantSku = ProductSkuGenerator.GenerateColorComboSku(product.SKU, color.SkuCode);
					string qrCode = _qrCodeService.GenerateQrBase64(variantSku);
					var newProductColor = new ProductColor
					{
						Id = Guid.NewGuid(),
						ProductId = product.Id, 
						ColorId = colorReq.ColorId,
						PriceSegmentId = colorReq.PriceSegmentId,
						Price = colorReq.Price,
						Sku = variantSku,
						QrCode = qrCode,
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
				throw new AppException($"Product Id = {id} not found", 404);
			_productRepository.Remove(product);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DisableProductAsync(Guid id)
		{
			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
				throw new AppException($"Product Id = {id} not found", 404);
			if (!product.IsActive)
				throw new AppException("Product already inactive", 400);
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
		public async Task<ProductResponse> GetByIdAsync(Guid id, bool? colorActive = false)
		{
		
			var product = await _productRepository.GetByIdAsync(id, colorActive);

			if (product == null)
				throw new KeyNotFoundException($"Product Id = {id} not found");

			
			if (colorActive == true && !product.IsActive)
				throw new AppException("Product is currently unavailable", 400);

			return MapToResponse(product);
		}

		//===Restore===
		public async Task<bool> RestoreProductAsync(Guid id)
		{
			var product = await _productRepository.GetByIdAsync(id);
			if (product == null)
				throw new AppException($"Product Id = {id} not found", 404);
			if (product.IsActive)
				throw new AppException("Product already active", 400);
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
				throw new AppException($"Product Id = {id} not found", 404);
			if (!string.IsNullOrWhiteSpace(request.Barcode) && request.Barcode != product.Barcode)
			{
				bool isDuplicate = await _productRepository.IsBarcodeExistsAsync(request.Barcode, id);
				if (isDuplicate)
				{
					throw new AppException($"Barcode '{request.Barcode}' Bar code is in use.", 400);
				}
				product.Barcode = request.Barcode.Trim();
			}
			// Update fields
			product.Description = request.Description ?? product.Description;
			product.BasePrice = request.BasePrice != default ? request.BasePrice : product.BasePrice;
			product.Brand = request.Brand ?? product.Brand;
			product.Material = request.Material ?? product.Material;
			product.OriginCountry = request.OriginCountry ?? product.OriginCountry;
			product.AgeRange = request.AgeRange ?? product.AgeRange;
			product.Weight = request.Weight ?? product.Weight;
			product.Length = request.Length ?? product.Length;
			product.Height = request.Height ?? product.Height;
			product.Width = request.Width ?? product.Width;
			
			if (request.IsConsignment.HasValue)
			{
				product.IsConsignment = request.IsConsignment.Value;
			}
			product.UpdatedAt = _dateTimeProvider.UtcNow;

			if (request.Colors != null && request.Colors.Any())
			{
				// Lấy danh sách ColorId gửi lên từ Frontend
				var requestedColorIds = request.Colors.Select(c => c.ColorId).ToList();

				// 1. XÓA những màu không còn nằm trong request 
				var colorsToRemove = product.ProductColors.Where(pc => !requestedColorIds.Contains(pc.ColorId)).ToList();
				if (colorsToRemove.Any())
				{
					_unitOfWork.Repository<ProductColor>().DeleteRange(colorsToRemove);
				}

				// 2. THÊM MỚI hoặc CẬP NHẬT
				foreach (var colorReq in request.Colors)
				{
					// Tìm xem màu này đã có trong sản phẩm chưa
					var existingColor = product.ProductColors.FirstOrDefault(pc => pc.ColorId == colorReq.ColorId);

					if (existingColor != null)
					{
						// NẾU ĐÃ CÓ -> Chỉ cập nhật giá, hình ảnh, 3D... KHÔNG đổi Id, KHÔNG gen lại SKU
						existingColor.PriceSegmentId = colorReq.PriceSegmentId;
						existingColor.Price = colorReq.Price;
						existingColor.ImageUrl = colorReq.ImageUrl;

						
					}
					else
					{
						// NẾU CHƯA CÓ -> Tạo mới hoàn toàn (Đoạn gen SKU chỉ dùng cho hàng mới)
						var colorEntity = await _colorRepository.GetByIdAsync(colorReq.ColorId)
							?? throw new AppException($"Color Id {colorReq.ColorId} not found", 404);

						var generatedSku = ProductSkuGenerator.GenerateColorComboSku(product.SKU, colorEntity.SkuCode);

						// Check trùng SKU cho hàng mới
						var skuExists = await _productColorRepository.ExistsBySkuAsync(generatedSku);
						if (skuExists)
							throw new AppException($"ProductColor SKU '{generatedSku}' already exists", 400);

						string qrCode = _qrCodeService.GenerateQrBase64(generatedSku);

						var newColor = new ProductColor
						{
							Id = Guid.NewGuid(),
							ColorId = colorReq.ColorId,
							ProductId = product.Id,
							PriceSegmentId = colorReq.PriceSegmentId,
							Sku = generatedSku,
							Price = colorReq.Price,
							QrCode = qrCode,
							ImageUrl = colorReq.ImageUrl,
							IsActive = true
						};

						await _unitOfWork.Repository<ProductColor>().AddAsync(newColor);
					}
				}
			}

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
				Barcode = product.Barcode,
				BasePrice = product.BasePrice,
				Brand = product.Brand,
				Material = product.Material,
				OriginCountry = product.OriginCountry,
				AgeRange = product.AgeRange,
				IsActive = product.IsActive,
				IsConsignment = product.IsConsignment,
				Weight = product.Weight,
				Length = product.Length,
				Height = product.Height,
				Width = product.Width,
				CreatedAt = product.CreatedAt,
				UpdatedAt = product.UpdatedAt,

				Colors = product.ProductColors
				.Where(c => c.IsActive)
				.Select(c => new ProductColorResponse
				{
					Id = c.Id,
					ProductId = product.Id,
					Sku = c.Sku,
					ColorId = c.ColorId,
					ColorName = c.Color.Name,
					Hexcode = c.Color.HexCode,
					PriceSegmentId = c.PriceSegmentId,
					Price = c.Price,
					QrCode = c.QrCode,
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

        public async Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetProductsPaginatedAsync(int pageNumber = 1, int pageSize = 10, bool? isActive = null, Guid? categoryId = null,string? searchItem = null)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 10;

            var (products, totalCount) = await _productRepository.GetProductsPaginatedAsync(pageNumber, pageSize, isActive,categoryId,searchItem);

            var productResponses = products.Select(MapToResponse);

            return (productResponses, totalCount);
        }
    }
}
