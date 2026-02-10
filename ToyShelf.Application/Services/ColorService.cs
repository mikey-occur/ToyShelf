using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Color.Request;
using ToyShelf.Application.Models.Color.Response;
using ToyShelf.Domain.Common.Product;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class ColorService : IColorService
	{
		private readonly IColorRepository _colorRepository;
		private readonly IUnitOfWork _unitOfWork;

		public ColorService(IColorRepository colorRepository, IUnitOfWork unitOfWork)
		{
			_colorRepository = colorRepository;
			_unitOfWork = unitOfWork;
		}

		public async Task<ColorResponse> CreateAsync(ColorRequest request)
		{

			bool exists = await _colorRepository.ExistsByNameOrHexAsync(request.Name, request.HexCode);
			if (exists)
			{
				throw new InvalidOperationException($"Color '{request.Name}' or Hex '{request.HexCode}' already exists.");
			}
			// XỬ LÝ SKU CODE (Phần mới thêm)
			string finalCode;

			// Nếu Admin có nhập Code -> Dùng code của Admin
			if (!string.IsNullOrWhiteSpace(request.SkuCode))
			{
				finalCode = request.SkuCode.Trim().ToUpper();
			}
			else
			{
				// Nếu Admin bỏ trống -> tự sinh (Black -> BK, Dark Blue -> DB)
				finalCode = ProductSkuGenerator.GetAutoCode(request.Name);
			}
			// Kiểm tra tính duy nhất của SKU Code nếu trùng thì thêm số vào đuôi
			string uniqueCode = finalCode;
			int counter = 1;

			while (await _colorRepository.ExistsBySkuCodeAsync(uniqueCode))
			{
				uniqueCode = $"{finalCode}{counter}"; // BL -> BL1 -> BL2
				counter++;
			}

			var color = new Color
			{
				Id = Guid.NewGuid(),
				Name = request.Name.Trim(),
				HexCode = request.HexCode.Trim().ToUpper(),
				SkuCode = uniqueCode
			};
			await _colorRepository.AddAsync(color);
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(color);
		}

		// ===== 2. GET ALL =====
		public async Task<IEnumerable<ColorResponse>> GetColorsAsync()
		{
			var colors = await _colorRepository.GetAllAsync();
			return colors.Select(c => MapToResponse(c));
		}

		// ===== 3. GET BY ID =====
		public async Task<ColorResponse> GetByIdAsync(Guid id)
		{
			var color = await _colorRepository.GetByIdAsync(id);
			if (color == null)
			{
				throw new AppException("Color not found.", 404); 
			}

			return MapToResponse(color);
		}

		public async Task DeleteAsync(Guid id)
		{
			var color = await _colorRepository.GetByIdAsync(id);
			if (color == null)
			{
				throw new KeyNotFoundException("Color not found.");
			}	
			_colorRepository.Remove(color);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ColorResponse> UpdateAsync(Guid id, ColorRequest request)
		{
			
			var color = await _colorRepository.GetByIdAsync(id);
			if (color == null)
			{
				throw new AppException("Color not found.", 404);
			}

			bool isDuplicate = await _colorRepository.IsDuplicateAsync(id, request.Name, request.HexCode);
			if (isDuplicate)
			{
				throw new InvalidOperationException($"Color Name '{request.Name}' or Hex '{request.HexCode}' is already taken by another color.");
			}

			color.Name = request.Name.Trim();
			color.HexCode = request.HexCode.Trim().ToUpper();
			await _unitOfWork.SaveChangesAsync();
			return MapToResponse(color);
		}

		// ===== MAPPER =====
		private ColorResponse MapToResponse(Color color)
		{
			return new ColorResponse
			{
				Id = color.Id,
				Name = color.Name,
				HexCode = color.HexCode,
				SkuCode = color.SkuCode
			};
		}
	}
}
