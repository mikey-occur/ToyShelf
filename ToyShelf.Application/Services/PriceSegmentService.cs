using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.PriceSegment.Request;
using ToyShelf.Application.Models.PriceSegment.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class PriceSegmentService : IPriceSegmentService
	{
		private readonly IPriceSegmentRepository _repo;
		private readonly IUnitOfWork _unitOfWork;

		public PriceSegmentService(IPriceSegmentRepository repo, IUnitOfWork unitOfWork)
		{
			_repo = repo;
			_unitOfWork = unitOfWork;
		}

		public async Task<PriceSegmentResponse> CreateAsync(PriceSegmentRequest request)
		{
			if (request.MaxPrice.HasValue && request.MinPrice >= request.MaxPrice.Value)
				throw new Exception("MinPrice must be less than MaxPrice");

			if (await _repo.ExistsByCodeAsync(request.Code))
				throw new Exception($"Segment Code '{request.Code}' already exists");

			var segment = new PriceSegment
			{
				Id = Guid.NewGuid(),
				Code = request.Code.Trim().ToUpper(),
				Name = request.Name.Trim(),
				MinPrice = request.MinPrice,
				MaxPrice = request.MaxPrice
			};

			await _repo.AddAsync(segment);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(segment);
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var segment = await _repo.GetByIdAsync(id) ?? throw new AppException("Not found", 404);

			bool isInUse = await _repo.IsSegmentInUseAsync(id);
			if (isInUse)
			{
				throw new InvalidOperationException("Cannot deleted because in use by product");
			}

			_repo.Remove(segment);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<PriceSegmentResponse>> GetAllAsync()
		{
			var segments = await _repo.GetAllAsync();
			return segments.OrderBy(s => s.MinPrice).Select(MapToResponse);
		}

		public async Task<PriceSegmentResponse> UpdateAsync(Guid id, PriceSegmentUpdateRequest request)
		{
			var segment = await _repo.GetByIdAsync(id)
			?? throw new AppException("Price Segment not found", 404);

			if (request.MaxPrice.HasValue && request.MinPrice >= request.MaxPrice.Value)
				throw new Exception("MinPrice must be less than MaxPrice");

			segment.Name = request.Name;
			segment.MinPrice = request.MinPrice;
			segment.MaxPrice = request.MaxPrice;
		
			_repo.Update(segment);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(segment);
		}

		private static PriceSegmentResponse MapToResponse(PriceSegment entity)
		{
			return new PriceSegmentResponse
			{
				Id = entity.Id,
				Code = entity.Code,
				Name = entity.Name,
				MinPrice = entity.MinPrice,
				MaxPrice = entity.MaxPrice
			};
		}

		public async Task<PriceSegmentResponse> GetByIdAsync(Guid id)
		{

			var price = await _repo.GetByIdAsync(id);
			if (price == null)
			{
				throw new AppException("Color not found.", 404);
			}

			return MapToResponse(price);
		}
	}
}
