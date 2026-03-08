using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IInventoryService
	{
		Task UpdateStockAfterPaymentAsync(Order order);
	}
}
