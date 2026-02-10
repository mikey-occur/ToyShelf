using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Common.Product
{
	public static class ProductSkuGenerator
	{
		private static readonly Dictionary<string, string> ColorMap =
			new(StringComparer.OrdinalIgnoreCase)
			{
				["WHITE"] = "W",
				["BLACK"] = "BK",
				["RED"] = "Rd",
				["BLUE"] = "Bl",
				["GREEN"] = "GY",
				["YELLOW"] = "YL",
				["PURPLE"] = "PP",
				["PINK"] = "PK",
				["ORANGE"] = "OR",
				["BROWN"] = "BR",
				["GRAY"] = "GY",
				["SILVER"] = "SL",
				["GOLD"] = "GD"
			};

		public static string GenerateColorComboSku(string productSku, string color)
		{
			if (string.IsNullOrWhiteSpace(productSku))
				throw new Exception("Product SKU is required");

			if (string.IsNullOrWhiteSpace(color))
				throw new Exception("Color is required");

			var normalizedColor = color.Trim().ToUpper();

			var colorCode = ColorMap.TryGetValue(normalizedColor, out var code)
				? code
				: normalizedColor.Substring(0, 1);

			return $"{productSku}-{colorCode}";
		}

		public static string GetAutoCode(string colorName)
		{
			if (string.IsNullOrWhiteSpace(colorName)) return "XX";

			// Chuẩn hóa: Viết hoa hết
			string cleanName = colorName.Trim().ToUpper();

			// 1. Ưu tiên tra từ điển (Map cứng cho các màu cơ bản)
			if (ColorMap.TryGetValue(cleanName, out var code)) return code;

			// 2. Tách chuỗi thành mảng các từ
			// Vd: "Dark Blue" -> ["DARK", "BLUE"]
			var words = cleanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			// 3. Trường hợp nhiều từ (Dark Blue, Light Green...)
			if (words.Length >= 2)
			{
				// Lấy ký tự đầu của mỗi từ ghép lại
				// "DARK" lấy 'D', "BLUE" lấy 'B' => "DB"
				return string.Join("", words.Select(w => w[0]));
			}

			// 4. Trường hợp 1 từ (Magenta, Cyan...)
			// Lấy 2 ký tự đầu: "MAGENTA" -> "MA"
			return cleanName.Length >= 2 ? cleanName.Substring(0, 2) : cleanName;
		}
	}
}
