using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.QRcode;

namespace ToyShelf.Infrastructure.Common.QrCode
{
	public class QrCodeService : IQrCodeService
	{
		public string GenerateQrBase64(string text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;

			using var qrGenerator = new QRCodeGenerator();
			using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
			// Tạo ảnh PNG trực tiếp từ dữ liệu QR
			var qrCode = new PngByteQRCode(qrCodeData);
			byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20); // 20 là kích thước điểm ảnh
			return $"data:image/png;base64,{Convert.ToBase64String(qrCodeAsPngByteArr)}";
		}
	}
}
