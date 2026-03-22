using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.MonthlySettlement.Response;

namespace ToyShelf.Application.IServices
{
	public interface IExportService
	{
		byte[] ExportSettlements(IEnumerable<MonthlySettlementResponse> settlements);
	}
}

