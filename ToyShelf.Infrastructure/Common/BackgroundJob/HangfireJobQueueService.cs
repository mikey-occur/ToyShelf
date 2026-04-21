using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Notifications;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Infrastructure.Common.BackgroundJob
{
	public class HangfireJobQueueService : IJobQueueService
	{
		private readonly IBackgroundJobClient _backgroundJobClient;

		public HangfireJobQueueService(IBackgroundJobClient backgroundJobClient)
		{
			_backgroundJobClient = backgroundJobClient;
		}

		public void EnqueuePaymentEmail(string toEmail, long orderCode)
		{
			_backgroundJobClient.Enqueue<IEmailService>(emailService =>
		        emailService.SendPaymentSuccessEmailAsync(toEmail, orderCode));
		}
	}
}
