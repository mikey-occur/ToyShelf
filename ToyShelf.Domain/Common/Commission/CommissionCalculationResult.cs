using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Common.Commission
{
	public record CommissionCalculationResult(decimal Rate, string SourceDescription);
}
