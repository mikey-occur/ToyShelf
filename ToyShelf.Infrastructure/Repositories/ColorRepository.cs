using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
	public class ColorRepository : GenericRepository<Color>, IColorRepository
	{
		public ColorRepository(ToyCabinDbContext context) : base(context)
		{
		}

		public async Task<bool> ExistsByNameOrHexAsync(string name, string hexCode)
		{
			return await _context.Colors
				.AnyAsync(c => c.Name == name || c.HexCode == hexCode);
		}

		public async Task<bool> IsDuplicateAsync(Guid id, string name, string hexCode)
		{
			// Trả về TRUE nếu:
			// Có thằng trùng Name hoặc HexCode
			// Thằng đó KHÔNG PHẢI là thằng đang sửa (Id != id)
			return await _context.Colors
				.AnyAsync(c => c.Id != id && (c.Name == name || c.HexCode == hexCode));
		}
	}
}
