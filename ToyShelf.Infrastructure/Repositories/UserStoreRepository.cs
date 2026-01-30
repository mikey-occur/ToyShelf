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
	public class UserStoreRepository : GenericRepository<UserStore>, IUserStoreRepository
	{
		public UserStoreRepository(ToyCabinDbContext context) : base(context) { }
	}
}
