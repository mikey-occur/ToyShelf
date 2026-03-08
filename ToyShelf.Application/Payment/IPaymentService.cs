using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Payment
{
	public interface IPaymentService
	{
		Task<string> CreatePaymentLink(Order order);
	}
}
