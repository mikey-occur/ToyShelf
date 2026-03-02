using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.City.Request;
using ToyShelf.Application.Models.City.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class CityService : ICityService
	{
		private readonly ICityRepository _cityRepository;
		private readonly IUnitOfWork _unitOfWork;

		public CityService(ICityRepository cityRepository, IUnitOfWork unitOfWork)
		{
			_cityRepository = cityRepository;
			_unitOfWork = unitOfWork;
		}

		// ===== 1. CREATE =====
		public async Task<CityResponse> CreateAsync(CityRequest request)
		{
			bool exists = await _cityRepository
				.ExistsByCodeOrNameAsync(request.Code.Trim().ToUpper(), request.Name.Trim());

			if (exists)
			{
				throw new InvalidOperationException(
					$"City Code '{request.Code}' or Name '{request.Name}' already exists.");
			}

			var city = new City
			{
				Id = Guid.NewGuid(),
				Code = request.Code.Trim().ToUpper(),
				Name = request.Name.Trim(),
				CreatedAt = DateTime.UtcNow
			};

			await _cityRepository.AddAsync(city);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(city);
		}

		// ===== 2. GET ALL =====
		public async Task<IEnumerable<CityResponse>> GetAsync()
		{
			var cities = await _cityRepository.GetAllAsync();
			return cities.Select(c => MapToResponse(c));
		}

		// ===== 3. GET BY ID =====
		public async Task<CityResponse> GetByIdAsync(Guid id)
		{
			var city = await _cityRepository.GetByIdAsync(id);

			if (city == null)
			{
				throw new AppException("City not found.", 404);
			}

			return MapToResponse(city);
		}

		// ===== 4. UPDATE =====
		public async Task<CityResponse> UpdateAsync(Guid id, CityRequest request)
		{
			var city = await _cityRepository.GetByIdAsync(id);

			if (city == null)
			{
				throw new AppException("City not found.", 404);
			}

			bool isDuplicate = await _cityRepository
				.IsDuplicateAsync(id, request.Code.Trim().ToUpper(), request.Name.Trim());

			if (isDuplicate)
			{
				throw new InvalidOperationException(
					$"City Code '{request.Code}' or Name '{request.Name}' already exists.");
			}

			city.Code = request.Code.Trim().ToUpper();
			city.Name = request.Name.Trim();
			city.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(city);
		}

		// ===== 5. DELETE =====
		public async Task DeleteAsync(Guid id)
		{
			var city = await _cityRepository.GetByIdAsync(id);

			if (city == null)
			{
				throw new AppException("City not found.", 404);
			}

			_cityRepository.Remove(city);
			await _unitOfWork.SaveChangesAsync();
		}

		// ===== MAPPER =====
		private CityResponse MapToResponse(City city)
		{
			return new CityResponse
			{
				Id = city.Id,
				Code = city.Code,
				Name = city.Name,
				CreatedAt = city.CreatedAt,
				UpdatedAt = city.UpdatedAt
			};
		}
	}

}
