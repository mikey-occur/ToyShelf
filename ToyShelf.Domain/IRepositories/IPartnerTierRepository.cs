using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Domain.IRepositories
{
	public interface IPartnerTierRepository : IGenericRepository<PartnerTier>
	{
		Task<bool> ExistsByNameAsync(string name);
		Task<bool> ExistsByPriorityAsync(int priority);
		Task<bool> IsTierInUseAsync(Guid id);
	}
}
