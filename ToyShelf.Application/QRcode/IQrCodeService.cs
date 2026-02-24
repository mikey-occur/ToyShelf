using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.QRcode
{
	public interface IQrCodeService
	{
		string GenerateQrBase64(string text);
	}
}
