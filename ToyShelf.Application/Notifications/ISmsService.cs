using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Notifications
{
	public interface ISmsService
	{
		Task SendPaymentSuccessSmsAsync(string phoneNumber, long orderCode);
	}
}
