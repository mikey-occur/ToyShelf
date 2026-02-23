using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IPriceSegmentRepository : IGenericRepository<PriceSegment>
	{
		Task<bool> ExistsByCodeAsync(string code);
		Task<bool> IsSegmentInUseAsync(Guid segmentId);
	}
}
