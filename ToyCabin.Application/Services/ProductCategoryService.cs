using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.ProductCategory.Request;
using ToyCabin.Application.Models.ProductCategory.Response;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
	public class ProductCategoryService : IProductCategoryService
	{
		private readonly IProductCategoryRepository _productCategoryRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		public ProductCategoryService(IProductCategoryRepository productCategoryRepository, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
		{
			_productCategoryRepository = productCategoryRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTimeProvider;
		}
		// ===== CREATE =====
		public async Task<ProductCategoryResponse> CreateCategoryAsync(ProductCategoryRequest request)
		{
			string code;

			if (request.ParentId == null)
			{
				code = ToCategoryCode(request.Name); 
			}
			else
			{
				var parent = await _productCategoryRepository
					.GetByIdAsync(request.ParentId.Value);

				if (parent == null)
					throw new Exception("Parent category not found");

				code = $"{parent.Code}-{ToCategoryCode(request.Code)}";
			}

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
		public async Task<bool> DeleteCategoryAsync(Guid id)
		{
			var category =  _productCategoryRepository.GetByIdAsync(id);
			if (category == null)
				throw new Exception("Category not found");

			_productCategoryRepository.Remove(category.Result);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DisableCategoryAsync(Guid id)
		{
			var category = await _productCategoryRepository.GetByIdAsync(id);

			if (category == null)
				throw new KeyNotFoundException($"Category not found. Id = {id}");

			if (!category.IsActive)
				throw new InvalidOperationException($"Category {id} is already disabled");

			category.IsActive = false;
			category.UpdatedAt = _dateTime.UtcNow;

			_productCategoryRepository.Update(category);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		// ===== GET =====

		public async Task<IEnumerable<ProductCategoryResponse>> GetAllCategoriesAsync()
		{
			var productCategories = await _productCategoryRepository.GetAllAsync();
			return productCategories.Select(MapToResponse);

		}

		public async Task<IEnumerable<ProductCategoryResponse>> GetinactiveCategoriesAsync()
		{
			var productCategories = await _productCategoryRepository
				.FindAsync(c => !c.IsActive);
			return productCategories.Select(MapToResponse);
		}

		public async Task<IEnumerable<ProductCategoryResponse>> GetActiveCategoriesAsync()
		{
			var productCategories = await _productCategoryRepository
				.FindAsync(c => c.IsActive);
			return productCategories.Select(MapToResponse);
		}

		public async Task<IEnumerable<ProductCategoryResponse>> GetCategoriesAsync(bool? isActive)
		{
			var productCategories = await _productCategoryRepository.GetProductCategoriesAsync(isActive);
			return productCategories.Select(MapToResponse);
		}
	
		public async Task<bool> RestoreCategoryAsync(Guid id)
		{
			var category = await _productCategoryRepository.GetByIdAsync(id);

			if (category == null)
				throw new KeyNotFoundException($"Category not found. Id = {id}");

			if (category.IsActive)
				throw new InvalidOperationException($"Category {id} is already active");

			category.IsActive = true;
			category.UpdatedAt = _dateTime.UtcNow;

			_productCategoryRepository.Update(category);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}
		// ===== UPDATE =====
		public async Task<ProductCategoryResponse?> UpdateCategoryAsync(Guid id, UpdateProductCategoryRequest request)
		{
			var category = await _productCategoryRepository.GetByIdAsync(id);
			if (category == null)
				throw new KeyNotFoundException($"Category not found. Id = {id}");

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
		//===== ConvertCode =====
		private string ToCategoryCode(string name)
		{
			return name
				.Trim()
				.Replace(" ", "-")
				.Replace("_", "-")
				.ToUpper();
		}

		
	}
}
