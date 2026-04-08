using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.MonthlySettlement.Response;
using ToyShelf.Application.Models.Product.Request;

namespace ToyShelf.Application.IServices
{
	public interface IExcelService
	{
		byte[] ExportSettlements(IEnumerable<MonthlySettlementResponse> settlements);
		List<ExcelProductImport> ReadProductExcel(Stream excelStream);
	}
}

