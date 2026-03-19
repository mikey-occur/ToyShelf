using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Order
{
	public class CreateOrderResponse
	{
		public long OrderCode { get; set; }
		public string PaymentUrl { get; set; } = string.Empty;
	}
}
