using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Common.Commission
{
	public class CommissionDomainService : ICommissionDomainService
	{
		public Task<CommissionCalculationResult> CalculateCommissionAsync(Guid partnerId, Guid productColorId, decimal soldPrice)
		{
			throw new NotImplementedException();
		}
	}
}
