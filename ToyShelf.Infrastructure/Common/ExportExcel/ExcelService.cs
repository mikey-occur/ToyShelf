using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.MonthlySettlement.Response;
using ToyShelf.Application.Models.Product.Request;

namespace ToyShelf.Infrastructure.Common.ExportExcel
{
	public class ExcelService : IExcelService
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

		public List<T> ImportGeneric<T>(Stream excelStream) where T : new()
		{
			var result = new List<T>();
			using var workbook = new XLWorkbook(excelStream);
			var worksheet = workbook.Worksheet(1);
			var rows = worksheet.RangeUsed().RowsUsed();

			// 1. Lấy danh sách Header ở dòng 1
			var headerRow = rows.First();
			var properties = typeof(T).GetProperties();
			var headerMapping = new Dictionary<int, System.Reflection.PropertyInfo>();

			for (int col = 1; col <= worksheet.LastColumnUsed().ColumnNumber(); col++)
			{
				var headerText = headerRow.Cell(col).GetString().Trim();
				var prop = properties.FirstOrDefault(p =>
				string.Equals(p.Name, headerText, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(p.Name, System.Text.RegularExpressions.Regex.Replace(headerText, @"\s+", ""), StringComparison.OrdinalIgnoreCase));
				if (prop != null)
				{
					headerMapping.Add(col, prop);
				}
			}

			// 2. Đọc dữ liệu từ dòng 2 trở đi
			foreach (var row in rows.Skip(1))
			{
				var item = new T();
				bool hasData = false;

				foreach (var mapping in headerMapping)
				{
					var cell = row.Cell(mapping.Key);
					if (!cell.IsEmpty())
					{
						var prop = mapping.Value;
						var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

						try
						{
							var value = Convert.ChangeType(cell.Value, targetType);
							prop.SetValue(item, value);
							hasData = true;
						}
						catch { /* Bỏ qua lỗi convert nếu dữ liệu rác */ }
					}
				}

				if (hasData) result.Add(item);
			}

			return result;
		}

		public List<ExcelProductImport> ReadProductExcel(Stream excelStream)
		{
			var result = new List<ExcelProductImport>();
			using var workbook = new XLWorkbook(excelStream);
			var worksheet = workbook.Worksheet(1);

			var usedRange = worksheet.RangeUsed();
			if (usedRange == null)
			{
				return result;
			}
			var rows = usedRange.RowsUsed().Skip(1);

			foreach (var row in rows)
			{
				if (row.Cell(2).IsEmpty() || row.Cell(14).IsEmpty())
					continue;

				result.Add(new ExcelProductImport
				{
					CategoryCode = row.Cell(1).GetString().Trim(),
					ProductName = row.Cell(2).GetString().Trim(),
					BasePrice = row.Cell(3).GetValue<decimal>(),
					Description = row.Cell(4).GetString().Trim(),
					Barcode = row.Cell(5).GetString().Trim(),
					Brand = row.Cell(6).GetString().Trim(),
					Material = row.Cell(7).GetString().Trim(),
					OriginCountry = row.Cell(8).GetString().Trim(),
					AgeRange = row.Cell(9).GetString().Trim(),
					Width = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetValue<decimal>(),
					Length = row.Cell(11).IsEmpty() ? null : row.Cell(11).GetValue<decimal>(),
					Height = row.Cell(12).IsEmpty() ? null : row.Cell(12).GetValue<decimal>(),
					Weight = row.Cell(13).IsEmpty() ? null : row.Cell(13).GetValue<decimal>(),
					ColorName = row.Cell(14).GetString().Trim(),
					ColorPrice = row.Cell(15).GetValue<decimal>(),
					ImageUrl = row.Cell(16).GetString().Trim(),
					Model3DUrl = row.Cell(17).GetString().Trim()
				});
			}
			return result;
		}

		public byte[] ExportGeneric<T>(IEnumerable<T> data, string sheetName = "ExportData")
		{
			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add(sheetName);

			
			var properties = typeof(T).GetProperties();

			// 1. TẠO HEADER (Dòng 1)
			for (int col = 0; col < properties.Length; col++)
			{
				var cell = worksheet.Cell(1, col + 1);

				
				cell.Value = properties[col].Name;

				
				cell.Style.Font.Bold = true;
				cell.Style.Fill.BackgroundColor = XLColor.LightGray;
				cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
			}

			// 2. ĐỔ DỮ LIỆU (Từ dòng 2 trở đi)
			if (data != null && data.Any())
			{
				int currentRow = 2;
				foreach (var item in data)
				{
					for (int col = 0; col < properties.Length; col++)
					{
						var cell = worksheet.Cell(currentRow, col + 1);

						
						var value = properties[col].GetValue(item);

						if (value != null)
						{
							// Phân loại data type để ClosedXML format cho đúng (số ra số, chữ ra chữ)
							if (value is DateTime dt)
							{
								cell.Value = dt;
								cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
							}
							else if (value is bool b)
							{
								cell.Value = b ? "True" : "False"; // Hoặc "Có" / "Không" tùy sếp
							}
							else if (value is int || value is decimal || value is double || value is float || value is long)
							{
								// Ép kiểu về double để Excel hiểu đây là cột Number (có thể tính tổng được)
								cell.Value = Convert.ToDouble(value);
							}
							else
							{
								cell.Value = value.ToString();
							}
						}
					}
					currentRow++;
				}
			}

		
			worksheet.Columns().AdjustToContents();
			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			return stream.ToArray();
		}
	}
}

