using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Infrastructure.Repositories
{
	public class PriceTableApplyRepository : GenericRepository<PriceTableApply>, IPriceTableApplyRepository
	{
		public PriceTableApplyRepository(Context.ToyShelfDbContext context) : base(context)
		{
		} 
	}
}
