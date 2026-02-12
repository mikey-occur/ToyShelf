using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.ProductCategory.Request;
using ToyShelf.Application.Models.ProductCategory.Response;
using ToyShelf.Application.Translation;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ProductCategoryService : IProductCategoryService
	{
		private readonly IProductCategoryRepository _productCategoryRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly ITranslationService _translationService;
		public ProductCategoryService(IProductCategoryRepository productCategoryRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, ITranslationService translationService)
		{
			_productCategoryRepository = productCategoryRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTimeProvider;
			_translationService = translationService;
		}
		// ===== CREATE =====
		public async Task<ProductCategoryResponse> CreateCategoryAsync(ProductCategoryRequest request)
		{
			string generatedPart = await _translationService.TranslateToCodeAsync(request.Name);
			string code;

			if (request.ParentId == null)
			{
				code = generatedPart; 
			}
			else
			{
				var parent = await _productCategoryRepository
					.GetByIdAsync(request.ParentId.Value);
				if (parent == null)
					throw new AppException("Parent category not found", 404);

				code = $"{parent.Code}-{generatedPart}";
			}
			var exists = await _productCategoryRepository.ExistsCodeAsync(code, request.ParentId);

			if (exists)
				throw new InvalidOperationException("Category code already exists");

			var category = new ProductCategory
			{
				Id = Guid.NewGuid(),
				Name = request.Name.Trim(),
				Code = code,
				ParentId = request.ParentId,
				Description = request.Description,
				IsActive = true,
				CreatedAt = _dateTime.UtcNow
			};

			await _productCategoryRepository.AddAsync(category);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(category);
		}
		// ===== Delete/Diasable =====
		public async Task DeleteCategoryAsync(Guid id)
		{
			var category = await _productCategoryRepository.GetByIdAsync(id);
			if (category == null)
				throw new AppException("Category not found", 404);

			var hasChild = await _productCategoryRepository.HasChildAsync(id);
			if (hasChild)
				throw new AppException(
					"Cannot delete category because it has child categories", 500);

			_productCategoryRepository.Remove(category);
			await _unitOfWork.SaveChangesAsync();

		}

		public async Task DisableCategoryAsync(Guid id)
		{
			var category = await _productCategoryRepository.GetByIdAsync(id);

			if (category == null)
				throw new AppException($"Category not found. Id = {id}", 404);

			if (!category.IsActive)
				throw new InvalidOperationException($"Category {id} is already disabled");

			category.IsActive = false;
			category.UpdatedAt = _dateTime.UtcNow;

			_productCategoryRepository.Update(category);
			await _unitOfWork.SaveChangesAsync();
		}

		// ===== GET =====
		public async Task<IEnumerable<ProductCategoryResponse>> GetCategoriesAsync(bool? isActive)
		{
			var productCategories = await _productCategoryRepository.GetProductCategoriesAsync(isActive);
			return productCategories.Select(MapToResponse);
		}
	
		public async Task RestoreCategoryAsync(Guid id)
		{
			var category = await _productCategoryRepository.GetByIdAsync(id);

			if (category == null)
				throw new AppException($"Category not found. Id = {id}", 404);

			if (category.IsActive)
				throw new InvalidOperationException($"Category {id} is already active");

			category.IsActive = true;
			category.UpdatedAt = _dateTime.UtcNow;

			_productCategoryRepository.Update(category);
			await _unitOfWork.SaveChangesAsync();
			
		}
		// ===== UPDATE =====
		public async Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid id, UpdateProductCategoryRequest request)
		{
			var category = await _productCategoryRepository.GetByIdAsync(id);
			if (category == null)
				throw new AppException($"Category not found. Id = {id}", 404);

			category.Name = request.Name.Trim();
			category.Description = request.Description;
			category.UpdatedAt = _dateTime.UtcNow;
			_productCategoryRepository.Update(category);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(category); 
		}

		// ===== MAPPER =====
		private ProductCategoryResponse MapToResponse(ProductCategory category)
		{
			return new ProductCategoryResponse
			{
				Id = category.Id,
				Name = category.Name,
				Code = category.Code,
				Description = category.Description,
				IsActive = category.IsActive,
				CreatedAt = category.CreatedAt,
				UpdatedAt = category.UpdatedAt
			};
		}
		
	}
}
