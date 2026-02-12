using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Color.Response;
using ToyShelf.Application.Models.PriceSegment.Request;
using ToyShelf.Application.Models.PriceSegment.Response;

namespace ToyShelf.Application.IServices
{
	public interface IPriceSegmentService
	{
		Task<IEnumerable<PriceSegmentResponse>> GetAllAsync();
		Task<PriceSegmentResponse> CreateAsync(PriceSegmentRequest request);
		Task<PriceSegmentResponse> UpdateAsync(Guid id, PriceSegmentUpdateRequest request);

		Task<PriceSegmentResponse> GetByIdAsync(Guid id);
		Task<bool> DeleteAsync(Guid id);
	}
}
