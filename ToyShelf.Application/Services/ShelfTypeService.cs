using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.ShelfType.Request;
using ToyShelf.Application.Models.ShelfType.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ShelfTypeService : IShelfTypeService
	{
		private readonly IShelfTypeRepository _shelfTypeRepository;
		private readonly IUnitOfWork _unitOfWork;

		public ShelfTypeService(IShelfTypeRepository shelfTypeRepository, IUnitOfWork unitOfWork)
		{
			_shelfTypeRepository = shelfTypeRepository;
			_unitOfWork = unitOfWork;
		}
		public async Task<ShelfTypeResponse> CreateAsync(ShelfTypeRequest request)
		{
			if (await _shelfTypeRepository.ExistsByNameAsync(request.Name.Trim()))
			{
				throw new AppException($"Mẫu kệ tên '{request.Name}' đã tồn tại!", 400);
			}

			var shelfType = new ShelfType
			{
				Id = Guid.NewGuid(),
				Name = request.Name.Trim(),
				ImageUrl = request.ImageUrl,
				Width = request.Width,
				Height = request.Height,
				Depth = request.Depth,
				TotalLevels = request.Levels?.Count ?? request.TotalLevels,
				SuitableProductCategoryTypes = request.SuitableProductCategoryTypes != null && request.SuitableProductCategoryTypes.Any()
						 ? string.Join(",", request.SuitableProductCategoryTypes)
						 : string.Empty,
				DisplayGuideline = request.DisplayGuideline,
				IsActive = true,

				// 3. Map mảng DTO Tầng sang Entity Con
				ShelfTypeLevels = request.Levels?.Select(l => new ShelfTypeLevel
				{
					Id = Guid.NewGuid(),
					Level = l.Level,
					Name = l.Name.Trim(),
					ClearanceHeight = l.ClearanceHeight,
					RecommendedCapacity = l.RecommendedCapacity,
					SuitableProductCategoryTypes = l.SuitableProductCategoryTypes != null && l.SuitableProductCategoryTypes.Any()
							 ? string.Join(",", l.SuitableProductCategoryTypes)
							 : string.Empty,
					DisplayGuideline = l.DisplayGuideline
				}).ToList() ?? new List<ShelfTypeLevel>()
			};

			await _shelfTypeRepository.AddAsync(shelfType);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(shelfType);
		}

		public async Task DeleteAsync(Guid id)
		{
			var shelfType = await _shelfTypeRepository.GetByIdAsync(id);
			if (shelfType == null)
				throw new AppException($"Not found shelf type with id = {id}", 404);

			_shelfTypeRepository.Remove(shelfType);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<bool> DisableAsync(Guid id)
		{
			var shelfType = await _shelfTypeRepository.GetByIdAsync(id);
			if (shelfType == null)
				throw new AppException($"ShelfType Id = {id} not found", 404);

			if (!shelfType.IsActive)
				throw new AppException("ShelfType already inactive", 400);

			shelfType.IsActive = false;

			_shelfTypeRepository.Update(shelfType);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		public async Task<bool> RestoreAsync(Guid id)
		{
			var shelfType = await _shelfTypeRepository.GetByIdAsync(id);
			if (shelfType == null)
				throw new AppException($"ShelfType Id = {id} not found", 404);

			if (shelfType.IsActive)
				throw new AppException("ShelfType already active", 400);

			shelfType.IsActive = true;

			_shelfTypeRepository.Update(shelfType);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		public async Task<IEnumerable<ShelfTypeResponse>> GetShelfTypesAsync(bool? isActive, string? searchName = null, string? categoryType = null)
		{

			var shelfTypes = await _shelfTypeRepository.GetShelfTypesAsync(isActive, searchName, categoryType);

			return shelfTypes.Select(MapToResponse);
		}

		public async Task<ShelfTypeResponse> GetByIdAsync(Guid id)
		{
			var shelfType = await _shelfTypeRepository.GetByIdWithLevelsAsync(id);
			if (shelfType == null)
				throw new AppException($"Không tìm thấy mẫu kệ với Id = {id}", 404);

			return MapToResponse(shelfType);
		}

		public async Task<ShelfTypeResponse> UpdateAsync(Guid id, ShelfTypeRequest request)
		{
			var shelfType = await _shelfTypeRepository.GetByIdAsync(id);
			if (shelfType == null)
				throw new AppException($"ShelfType Id = {id} not found", 404);

			if (!string.IsNullOrWhiteSpace(request.Name) && shelfType.Name != request.Name.Trim())
			{
				if (await _shelfTypeRepository.ExistsByNameAsync(request.Name.Trim(), id))
					throw new AppException($"Mẫu kệ tên '{request.Name}' đã tồn tại!", 400);
			}

			shelfType.Name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name.Trim() : shelfType.Name;
			shelfType.ImageUrl = request.ImageUrl ?? shelfType.ImageUrl;
			shelfType.Width = request.Width != default ? request.Width : shelfType.Width;
			shelfType.Height = request.Height != default ? request.Height : shelfType.Height;
			shelfType.Depth = request.Depth != default ? request.Depth : shelfType.Depth;
			shelfType.TotalLevels = request.TotalLevels != default ? request.TotalLevels : shelfType.TotalLevels;
			shelfType.DisplayGuideline = request.DisplayGuideline ?? shelfType.DisplayGuideline;

			if (request.SuitableProductCategoryTypes != null)
			{
				shelfType.SuitableProductCategoryTypes = request.SuitableProductCategoryTypes.Any()
					? string.Join(",", request.SuitableProductCategoryTypes)
					: string.Empty;
			}

			if (request.Levels != null && request.Levels.Any())
			{
			
				var requestedLevels = request.Levels.Select(l => l.Level).ToList();

				 
				var levelsToRemove = shelfType.ShelfTypeLevels.Where(l => !requestedLevels.Contains(l.Level)).ToList();
				if (levelsToRemove.Any())
				{
				
					_unitOfWork.Repository<ShelfTypeLevel>().DeleteRange(levelsToRemove);
				}

				foreach (var levelReq in request.Levels)
				{
					
					var existingLevel = shelfType.ShelfTypeLevels.FirstOrDefault(l => l.Level == levelReq.Level);

					if (existingLevel != null)
					{
						
						existingLevel.Name = !string.IsNullOrWhiteSpace(levelReq.Name) ? levelReq.Name.Trim() : existingLevel.Name;
						existingLevel.ClearanceHeight = levelReq.ClearanceHeight != default ? levelReq.ClearanceHeight : existingLevel.ClearanceHeight;
						existingLevel.RecommendedCapacity = levelReq.RecommendedCapacity != default ? levelReq.RecommendedCapacity : existingLevel.RecommendedCapacity;
						if (levelReq.SuitableProductCategoryTypes != null)
						{
							existingLevel.SuitableProductCategoryTypes = levelReq.SuitableProductCategoryTypes.Any()
								? string.Join(", ", levelReq.SuitableProductCategoryTypes)
								: string.Empty;
						}
						existingLevel.DisplayGuideline = levelReq.DisplayGuideline ?? existingLevel.DisplayGuideline;
					}
					else
					{
						var newLevel = new ShelfTypeLevel
						{
							Id = Guid.NewGuid(),
							ShelfTypeId = shelfType.Id,
							Level = levelReq.Level,
							Name = levelReq.Name.Trim(),
							ClearanceHeight = levelReq.ClearanceHeight,
							RecommendedCapacity = levelReq.RecommendedCapacity,
							SuitableProductCategoryTypes = levelReq.SuitableProductCategoryTypes != null && levelReq.SuitableProductCategoryTypes.Any()
									? string.Join(", ", levelReq.SuitableProductCategoryTypes)
									: string.Empty,
							DisplayGuideline = levelReq.DisplayGuideline
						};

						await _unitOfWork.Repository<ShelfTypeLevel>().AddAsync(newLevel);
					}
				}
			}

			_shelfTypeRepository.Update(shelfType);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(shelfType);
		}


		private static ShelfTypeResponse MapToResponse(ShelfType shelfType)
		{
			return new ShelfTypeResponse
			{
				Id = shelfType.Id,	
				Name = shelfType.Name,
				ImageUrl = shelfType.ImageUrl,
				Width = shelfType.Width,
				Height = shelfType.Height,
				Depth = shelfType.Depth,
				TotalLevels = shelfType.TotalLevels,
				SuitableProductCategoryTypes = shelfType.SuitableProductCategoryTypes, 
				DisplayGuideline = shelfType.DisplayGuideline, 
				IsActive = shelfType.IsActive,

				Levels = shelfType.ShelfTypeLevels?.Select(l => new ShelfTypeResponse.ShelfTypeLevelResponse
				{
					Level = l.Level,
					Name = l.Name,
					ClearanceHeight = l.ClearanceHeight,
					RecommendedCapacity = l.RecommendedCapacity,
					SuitableProductCategoryTypes = l.SuitableProductCategoryTypes, 
					DisplayGuideline = l.DisplayGuideline 
				}).ToList() ?? new List<ShelfTypeResponse.ShelfTypeLevelResponse>()
			};
		}

	
	}
}
