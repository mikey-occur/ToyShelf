using GTranslate.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ToyShelf.Application.Translation;

namespace ToyShelf.Infrastructure.Common.Translation
{
	public class TranslationService : ITranslationService
	{
		private readonly AggregateTranslator _translator;

		public TranslationService()
		{
			_translator = new AggregateTranslator();
		}

		public async Task<string> TranslateToCodeAsync(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) return string.Empty;

			string textToFormat = name;

			// 1. Nếu có dấu Tiếng Việt -> Dịch sang Anh
			if (ContainsVietnamese(name))
			{
				try
				{
					// Dịch sang tiếng Anh
					var result = await _translator.TranslateAsync(name, "en");
					textToFormat = result.Translation;
				}
				catch
				{
					// Nếu lỗi mạng, fallback về xóa dấu
					textToFormat = RemoveDiacritics(name);
				}
			}
			return SanitizeAndFormat(textToFormat);
		}

		private string SanitizeAndFormat(string text)
		{
			
			string str = RemoveDiacritics(text).ToLower();
			str = str.Replace(" ", "-").Replace("_", "-");
			// Chỉ giữ lại a-z, 0-9 và dấu gạch ngang
			str = Regex.Replace(str, @"[^a-z0-9-]", "");
			// Xóa gạch ngang thừa (vd: -- thành -)
			str = Regex.Replace(str, @"-+", "-");
			// Cắt 2 đầu và viết hoa
			return str.Trim('-').ToUpper();
		}

		private bool ContainsVietnamese(string text)
		{
			return Regex.IsMatch(text, @"[áàảãạăắằẳẵặâấầẩẫậéèẻẽẹêếềểễệíìỉĩịóòỏõọôốồổỗộơớờởỡợúùủũụưứừửữựýỳỷỹỵđ]", RegexOptions.IgnoreCase);
		}

		private string RemoveDiacritics(string text)
		{
			string normalizedString = text.Normalize(NormalizationForm.FormD);
			StringBuilder stringBuilder = new StringBuilder();

			foreach (char c in normalizedString)
			{
				System.Globalization.UnicodeCategory unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}
	}
}
