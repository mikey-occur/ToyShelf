using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Common.Product
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
	}
}
