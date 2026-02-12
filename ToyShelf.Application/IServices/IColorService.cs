using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Color.Request;
using ToyShelf.Application.Models.Color.Response;
using ToyShelf.Application.Models.Store.Request;
using ToyShelf.Application.Models.Store.Response;

namespace ToyShelf.Application.IServices
{
	public interface IColorService
	{
		// CREATE
		Task<ColorResponse> CreateAsync(ColorRequest request);

		// GET
		Task<IEnumerable<ColorResponse>> GetColorsAsync();
		Task<ColorResponse> GetByIdAsync(Guid id);

		// UPDATE
		Task<ColorResponse> UpdateAsync(Guid id,  ColorRequest request);

		// DELETE
		Task DeleteAsync(Guid id);
	}
}
