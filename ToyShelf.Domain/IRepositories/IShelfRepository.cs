using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
    public interface IShelfRepository : IGenericRepository<Shelf>
    {
        Task<(IEnumerable<Shelf> Items, int TotalCount)> GetShelvesPaginatedAsync(
       int pageNumber = 1,
       int pageSize = 10,
       ShelfStatus? status = null);
    }
}
