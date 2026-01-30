using Microsoft.EntityFrameworkCore;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
    public class CabinRepository : GenericRepository<Cabin>, ICabinRepository
    {
        public CabinRepository(ToyCabinDbContext context) : base(context){}

       
    }
    
}