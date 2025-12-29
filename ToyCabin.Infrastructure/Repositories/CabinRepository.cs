using Microsoft.EntityFrameworkCore;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;
using ToyCabin.Infrastructure.Context;

namespace ToyCabin.Infrastructure.Repositories
{
    public class CabinRepository : GenericRepository<Cabin>, ICabinRepository
    {
        public CabinRepository(ToyCabinDbContext context) : base(context){}

       
    }
    
}