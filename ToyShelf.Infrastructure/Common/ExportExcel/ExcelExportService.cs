using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.MonthlySettlement.Response;

namespace ToyShelf.Infrastructure.Common.ExportExcel
{
	public class ExcelExportService : IExportService
	{
		public byte[] ExportSettlements(IEnumerable<MonthlySettlementResponse> settlements)
		{
			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("Chốt Sổ Tháng");

			// 1. Đổ màu và ghi Tiêu đề
			var headers = new string[] { "Mã Đối Tác", "Tên Đối Tác", "Tháng", "Năm", "Tổng Số hàng bán được", "Tổng Tiền Bán", "Tổng Hoa Hồng", "Khấu Trừ", "Thực Nhận", "Trạng Thái", "Ngày Tạo" };
			for (int i = 0; i < headers.Length; i++)
			{
				var cell = worksheet.Cell(1, i + 1);
				cell.Value = headers[i];
				cell.Style.Font.Bold = true;
				cell.Style.Fill.BackgroundColor = XLColor.LightGray;
			}

			// 2. Đổ dữ liệu
			int currentRow = 2;
			foreach (var item in settlements)
			{
				worksheet.Cell(currentRow, 1).Value = item.PartnerCode;
				worksheet.Cell(currentRow, 2).Value = item.PartnerName;
				worksheet.Cell(currentRow, 3).Value = item.Month;
				worksheet.Cell(currentRow, 4).Value = item.Year;
				worksheet.Cell(currentRow, 5).Value = item.TotalItems;
				worksheet.Cell(currentRow, 6).Value = item.TotalSalesAmount;
				worksheet.Cell(currentRow, 7).Value = item.TotalCommissionAmount;
				worksheet.Cell(currentRow, 8).Value = item.DeductionAmount;
				worksheet.Cell(currentRow, 9).Value = item.FinalAmount;
				worksheet.Cell(currentRow, 10).Value = item.Status;
				worksheet.Cell(currentRow, 11).Value = item.CreatedAt.ToString("dd/MM/yyyy HH:mm");
				currentRow++;
			}

			// 3. Căn chỉnh tự động cho đẹp
			worksheet.Columns().AdjustToContents();

			// 4. Đóng gói thành file
			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}
	}
}
