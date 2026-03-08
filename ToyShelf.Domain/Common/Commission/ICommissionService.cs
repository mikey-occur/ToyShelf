using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Common.Commission
{
	public interface ICommissionService
	{
		Task<CommissionCalculationResult> CalculateCommissionAsync(Guid partnerId, Guid productColorId, decimal soldPrice);
	}
}
