using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.CommissionHistory.Response;
using ToyShelf.Application.Models.MonthlySettlement.Response;
using ToyShelf.Application.Models.PriceTable.Response;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class MonthlySettlementService : IMonthlySettlementService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommissionHistoryRepsitory _commissionHistoryRepsitory;
		private readonly IMonthlySettlementRepository _settlementRepository;

		public MonthlySettlementService(IUnitOfWork unitOfWork, ICommissionHistoryRepsitory commissionHistoryRepsitory, IMonthlySettlementRepository settlementRepository)
		{
			_unitOfWork = unitOfWork;
			_commissionHistoryRepsitory = commissionHistoryRepsitory;
			_settlementRepository = settlementRepository;
		}

		// Tổng kết hoá đơn tháng
		public async Task<List<MonthlySettlementResponse>> GenerateMonthlySettlementAsync(int month, int year)
		{
			// 1. Mốc thời gian: < Giây đầu tiên của tháng tiếp theo
			var endOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
			// 2. Lấy dữ liệu qua Repo (Nhớ Include Partner để xíu nữa Map tên ra DTO)
			var unmappedHistories = await _commissionHistoryRepsitory.GetUnsettledHistoriesAsync(endOfMonth);

			if (!unmappedHistories.Any()) return new List<MonthlySettlementResponse>();

			// 3. Gom nhóm theo PartnerId
			var groupedByPartner = unmappedHistories.GroupBy(ch => ch.PartnerId);

			var newSettlements = new List<MonthlySettlement>();
			var responseList = new List<MonthlySettlementResponse>();

			foreach (var group in groupedByPartner)
			{
				var settlement = new MonthlySettlement
				{
					Id = Guid.NewGuid(),
					PartnerId = group.Key,
					Month = month,
					Year = year,
					TotalItems = group.Count(),
					TotalCommissionAmount = group.Sum(ch => ch.CommissionAmount),
					Status = "PENDING",
					CreatedAt = DateTime.UtcNow,
					Partner = group.First().Partner
				};
				foreach (var history in group)
				{
					history.MonthlySettlementId = settlement.Id;
					_unitOfWork.Repository<CommissionHistory>().Update(history);
				}

				newSettlements.Add(settlement);
				responseList.Add(MapToResponse(settlement));
			}

			// 5. Lưu vào Database
			await _unitOfWork.Repository<MonthlySettlement>().AddRangeAsync(newSettlements);
			await _unitOfWork.SaveChangesAsync();

			return responseList;
		}

		// lấy chi tiết kê khai tháng
		public async Task<MonthlySettlementResponse?> GetByIdAsync(Guid id)
		{
			var settlement = await _settlementRepository.GetSettlementWithDetailsByIdAsync(id);

			if (settlement == null) throw new AppException($"Settlement Not Found.", 404); ;

			var response = MapToResponse(settlement);
			response.Histories = settlement.CommissionHistories.Select(ch => new CommissionHistoryResponse
			{
				Id = ch.Id,
				OrderItemId = ch.OrderItemId,
				AppliedRate = ch.AppliedRate,
				CommissionAmount = ch.CommissionAmount,
				CreatedAt = ch.CreatedAt
			}).ToList();

			return response;
		}

		public async Task<bool> PayAsync(Guid id)
		{
			var settlement = await _unitOfWork.Repository<MonthlySettlement>().GetByIdAsync(id);

			if (settlement == null || settlement.Status == "PAID")
			{
				return false;
			}
			settlement.Status = "PAID";
			settlement.PaidAt = DateTime.UtcNow;
			_unitOfWork.Repository<MonthlySettlement>().Update(settlement);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

		public async Task<IEnumerable<MonthlySettlementResponse>> GetAllAsync()
		{
			var settlements = await _settlementRepository.GetAllAsync();
			return settlements.Select(MapToResponse);
		}

		private static MonthlySettlementResponse MapToResponse(MonthlySettlement settlement)
		{
			return new MonthlySettlementResponse
			{
				Id = settlement.Id,
				PartnerId = settlement.PartnerId,
				PartnerName = settlement.Partner?.CompanyName,
				Month = settlement.Month,
				Year = settlement.Year,
				TotalItems = settlement.TotalItems,
				TotalCommissionAmount = settlement.TotalCommissionAmount,
				Status = settlement.Status,
				CreatedAt = settlement.CreatedAt
			};
		}

		

	}
}
