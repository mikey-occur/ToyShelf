using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;
using ToyCabin.Infrastructure.Context;

namespace ToyCabin.Infrastructure.Repositories
{
	public class PartnerRepository : GenericRepository<Partner>, IPartnerRepository
	{
		public PartnerRepository(ToyCabinDbContext context) : base(context) { }
	}
}
