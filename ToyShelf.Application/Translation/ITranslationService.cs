using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Translation
{
	public interface ITranslationService
	{
		Task<string> TranslateToCodeAsync(string name);
	}
}
