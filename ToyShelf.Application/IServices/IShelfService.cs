using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.Models.Shelf.Request;
using ToyShelf.Application.Models.Shelf.Response;

namespace ToyShelf.Application.IServices
{
    public interface IShelfService
    {
        Task<ShelfResponse> CreateAsync(CreateShelfRequest request);
        Task<IEnumerable<ShelfResponse>> GetAllAsync();
        Task<ShelfResponse> GetByIdAsync(Guid id);
        Task<ShelfResponse> UpdateAsync(Guid id, UpdateShelfRequest request);
        Task<PaginatedResult<ShelfResponse>> GetPaginatedAsync(int pageNumber = 1, int pageSize = 10, string? status = null);
        Task DeleteAsync(Guid id);
    }
}
