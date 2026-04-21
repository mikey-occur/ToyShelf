using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Notifications
{
	public interface IJobQueueService
	{
		void EnqueuePaymentEmail(string Email, long orderCode);
	}
}
