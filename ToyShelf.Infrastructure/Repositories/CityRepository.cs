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
	public class CityRepository : GenericRepository<City>, ICityRepository
	{
		public CityRepository(ToyShelfDbContext context) : base(context) { }

		public async Task<bool> ExistsByCodeOrNameAsync(string code, string name)
		{
			return await _context.Cities
				.AnyAsync(c => c.Code == code || c.Name == name);
		}

		public async Task<bool> IsDuplicateAsync(Guid id, string code, string name)
		{
			return await _context.Cities
				.AnyAsync(c => c.Id != id &&
							  (c.Code == code || c.Name == name));
		}
	}
}
