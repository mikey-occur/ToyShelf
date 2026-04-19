using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Notifications;

namespace ToyShelf.Infrastructure.Common.BackgroundJob
{
	public class HangfireJobQueueService : IJobQueueService
	{
		private readonly IBackgroundJobClient _backgroundJobClient;

		public HangfireJobQueueService(IBackgroundJobClient backgroundJobClient)
		{
			_backgroundJobClient = backgroundJobClient;
		}

		public void EnqueuePaymentSms(string phoneNumber, long orderCode)
		{
			_backgroundJobClient.Enqueue<ISmsService>(sms =>
				sms.SendPaymentSuccessSmsAsync(phoneNumber, orderCode));
		}
	}
}
