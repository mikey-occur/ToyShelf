using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.City.Request;
using ToyShelf.Application.Models.City.Response;

namespace ToyShelf.Application.IServices
{
	public interface ICityService
	{
		Task<CityResponse> CreateAsync(CityRequest request);
		Task<IEnumerable<CityResponse>> GetAsync();
		Task<CityResponse> GetByIdAsync(Guid id);
		Task<CityResponse> UpdateAsync(Guid id, CityRequest request);
		Task DeleteAsync(Guid id);
	}
}
