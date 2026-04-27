using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.CommissionHistory.Response;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class CommissionHistoryService : ICommissionHistoryService
	{
		private readonly ICommissionHistoryRepsitory _commissionRepo;

		public CommissionHistoryService(ICommissionHistoryRepsitory commissionRepo)
		{
			_commissionRepo = commissionRepo;
		}

		public async Task<(IEnumerable<CommissionHistoryResponse> Items, int TotalCount)> GetHistoriesPaginatedAsync(
			int pageNumber = 1,
			int pageSize = 10,
			Guid? partnerId = null,
			string? searchItem = null, Guid? storeId = null,
			DateTime? fromDate = null,
			DateTime? toDate = null)
		{

			var (entities, totalCount) = await _commissionRepo.GetHistoriesPaginatedAsync(pageNumber, pageSize, partnerId, searchItem, storeId, fromDate, toDate);

			var items = entities.Select(c => new CommissionHistoryResponse
			{
				Id = c.Id,
				OrderItemId = c.OrderItemId,
				AppliedRate = c.AppliedRate,
				CommissionAmount = c.CommissionAmount,
				CreatedAt = c.CreatedAt,
				Quantity = c.OrderItem.Quantity,
				OrderCode = c.OrderItem.Order.OrderCode,
				PaymentMethod = c.OrderItem.Order.PaymentMethod,
				OrderDate = c.OrderItem.Order.CreatedAt
			}).ToList(); 

			return (items, totalCount);
		}
	}
}
