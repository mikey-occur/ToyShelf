using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Common;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.CommissionHistory.Response;
using ToyShelf.Application.Models.MonthlySettlement.Request;
using ToyShelf.Application.Models.MonthlySettlement.Response;
using ToyShelf.Application.Models.Notification.Request;
using ToyShelf.Application.Models.PriceTable.Response;
using ToyShelf.Application.Notifications;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class MonthlySettlementService : IMonthlySettlementService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommissionHistoryRepsitory _commissionHistoryRepsitory;
		private readonly IMonthlySettlementRepository _settlementRepository;
		private readonly IExcelService _exportService;
		private readonly INotificationService _notificationService;
		private readonly INotificationBroadcaster _notificationBroadcaster;
		private readonly IUserRepository _userRepository;
		private readonly ILogger<MonthlySettlementService> _logger;
        private readonly IEmailService _emailService;
        public MonthlySettlementService(IUnitOfWork unitOfWork, ICommissionHistoryRepsitory commissionHistoryRepsitory, IMonthlySettlementRepository settlementRepository, IExcelService exportService, INotificationService notificationService, INotificationBroadcaster notificationBroadcaster, IUserRepository userRepository, ILogger<MonthlySettlementService> logger, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_commissionHistoryRepsitory = commissionHistoryRepsitory;
			_settlementRepository = settlementRepository;
			_exportService = exportService;
			_notificationService = notificationService;
			_notificationBroadcaster = notificationBroadcaster;
			_userRepository = userRepository;
            _logger = logger;
		    _emailService = emailService;
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
				var totalCommission = group.Sum(ch => ch.CommissionAmount);
				var totalItems = group.Sum(ch => ch.OrderItem.Quantity);
				var totalSales = group.Sum(ch => ch.OrderItem.Price * ch.OrderItem.Quantity);
				var settlement = new MonthlySettlement
				{
					Id = Guid.NewGuid(),
					PartnerId = group.Key,
					Month = month,
					Year = year,
					DeductionAmount = 0,                  
					Note = null,
					TotalItems = totalItems,
					TotalSalesAmount = totalSales,
					TotalCommissionAmount = totalCommission,
					FinalAmount = totalCommission,
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

            if (settlement == null) throw new AppException("Settlement Not Found.", 404);

            var response = MapToResponse(settlement);

            
            response.DailySummaries = settlement.CommissionHistories
                .Where(ch => ch.OrderItem?.Order != null) 
                .GroupBy(ch => ch.OrderItem!.Order!.CreatedAt.Date)
                .Select(dayGroup => new DailySettlementSummaryResponse
                {
                    Date = dayGroup.Key.ToString("yyyy-MM-dd"),

                    TotalOrders = dayGroup.Select(ch => ch.OrderItem!.OrderId).Distinct().Count(),
                    TotalProductsSold = dayGroup.Sum(ch => ch.OrderItem!.Quantity),
                    TotalSalesAmount = dayGroup.Sum(ch => ch.SalesAmount),
                    TotalCommissionAmount = dayGroup.Sum(ch => ch.CommissionAmount),

              
                    Transactions = dayGroup
                        .GroupBy(ch => ch.OrderItem!.Order)
                        .Select(orderGroup => new OrderTransactionResponse
                        {
                           
                            Id = orderGroup.Key!.Id,
                            OrderId = orderGroup.Key.Id,
                            OrderCode = orderGroup.Key.OrderCode,
                            TotalAmount = orderGroup.Key.TotalAmount, // Tổng tiền hóa đơn

                            TotalCommission = orderGroup.Sum(x => x.CommissionAmount),

                            Status = orderGroup.Key.Status,
                            CreatedAt = orderGroup.Key.CreatedAt,
							IsLocked = orderGroup.Key.IsLocked,
                            OrderDate = orderGroup.Key.CreatedAt
                        })
                        .OrderByDescending(t => t.OrderDate) 
                        .ToList()
                })
                .OrderByDescending(d => d.Date)
                .ToList();

            return response;
        }

		public async Task<bool> PayAsync(Guid id, string transferReceiptUrl)
		{
            var settlement = await _unitOfWork.Repository<MonthlySettlement>().GetByIdAsync(id);

           
            if (settlement == null)
            {
                throw new AppException("Không tìm thấy phiếu đối soát này!", 404);
            }

           
            if (settlement.Status == "PAID" || settlement.Status == "RECEIVED")
            {
                throw new AppException($"Phiếu này đã được thanh toán hoặc xác nhận (Trạng thái hiện tại: {settlement.Status})", 400);
            }

            var partnerAdmin = await _userRepository.GetPartnerAdminAsync(settlement.PartnerId);
            if (partnerAdmin == null)
            {
                throw new AppException("Không tìm thấy thông tin Quản lý của Đối tác (PartnerAdmin) để gửi thông báo", 404);
            }

            var partner = await _unitOfWork.Repository<Partner>().GetByIdAsync(settlement.PartnerId);
            if (partner == null)
            {
                throw new AppException("Không tìm thấy dữ liệu Công ty của đối tác này!", 404);
            }

            settlement.Status = "PAID";
            settlement.PaidAt = DateTime.UtcNow;
            settlement.TransferReceiptUrl = transferReceiptUrl;
            _unitOfWork.Repository<MonthlySettlement>().Update(settlement);

            var request = new CreateNotificationRequest
            {
                UserId = partnerAdmin.Id,
                Title = "Thanh toán đối soát thành công",
                Content = "Kỳ đối soát tháng này đã được chuyển khoản. Vui lòng bấm Xác Nhận khi bạn đã nhận được."
            };
            await _notificationService.CreateNotificationAsync(request);

         
            await _unitOfWork.SaveChangesAsync();

        
            try
            {
                await _notificationBroadcaster.SendNotificationToUserAsync(
                    partnerAdmin.Id,
                    request.Title,
                    request.Content
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi khi push notification realtime tới PartnerAdmin {PartnerAdminId} cho Settlement {SettlementId}. Lỗi: {ErrorMessage}",
                    partnerAdmin.Id,
                    id,
                    ex.Message
                );
            }

            try
            {
                await _emailService.SendSettlementPaymentEmailAsync(
                    partnerAdmin.Email, 
                    settlement,
                    partner,
                    partnerAdmin
                );
            }
            catch (Exception ex)
            {
                
                _logger.LogError(
                    ex,
                    "Lỗi khi gửi email Biên lai thanh toán tới {Email} cho phiếu {SettlementId}. Lỗi: {ErrorMessage}",
                    partnerAdmin.Email,
                    id,
                    ex.Message
                );
            }

            return true;
        }

		public async Task<IEnumerable<MonthlySettlementResponse>> GetAllFilterAsync(SettlementFilterRequest filter)
		{
			var settlements = await _settlementRepository.GetFilteredSettlementsAsync(
							filter.Year,
							filter.Month,
							filter.PartnerId,
							filter.Status );

			return settlements.Select(MapToResponse);
		}

		public async Task<IEnumerable<MonthlySettlementResponse>> GetAllAsync()
		{
			var settlements = await _settlementRepository.GetAllAsync();
			return settlements.Select(MapToResponse);
		}

		public async Task GenerateLastMonthSettlementAutoAsync()
		{
			var lastMonth = DateTime.UtcNow.AddMonths(-1);
			await GenerateMonthlySettlementAsync(lastMonth.Month, lastMonth.Year);
		}

		public async Task<MonthlySettlementResponse> UpdateDeductionAsync(Guid id, decimal deductionAmount, string note)
		{
			// 1. Tìm cái phiếu chốt sổ dưới DB
			var settlement = await _unitOfWork.Repository<MonthlySettlement>().GetByIdAsync(id);

			// 2. Bắt lỗi chặt chẽ
			if (settlement == null)
				throw new AppException("Not found settlement.", 404);

			if (settlement.Status == "PAID")
				throw new AppException("Have already paid", 400);

			if (deductionAmount > settlement.TotalCommissionAmount)
				throw new AppException("debuction no large than total amount", 400);

			// 3. Cập nhật số tiền trừ và ghi chú
			settlement.DeductionAmount = deductionAmount;
			settlement.Note = note;

			// 4. Tính toán lại Tổng thực nhận (FinalAmount)
			settlement.FinalAmount = settlement.TotalCommissionAmount - deductionAmount;

			// 5. Lưu đè xuống Database
			_unitOfWork.Repository<MonthlySettlement>().Update(settlement);
			await _unitOfWork.SaveChangesAsync();

			return MapToResponse(settlement);
		}



		private static MonthlySettlementResponse MapToResponse(MonthlySettlement settlement)
		{
			return new MonthlySettlementResponse
			{
				Id = settlement.Id,
				PartnerId = settlement.PartnerId,
				PartnerName = settlement.Partner?.CompanyName,
				PartnerCode = settlement.Partner?.Code ?? string.Empty,
				BankName = settlement.Partner?.BankName,
				BankAccountName = settlement.Partner?.BankAccountName,
				BankAccountNumber = settlement.Partner?.BankAccountNumber,
                Month = settlement.Month,
				Year = settlement.Year,
				TotalItems = settlement.TotalItems,
				TotalSalesAmount = settlement.TotalSalesAmount,
				TotalCommissionAmount = settlement.TotalCommissionAmount,
				DeductionAmount = settlement.DeductionAmount,
				FinalAmount = settlement.FinalAmount,
				Note = settlement.Note,
				Status = settlement.Status,
				CreatedAt = settlement.CreatedAt
			};
		}

		public async Task<byte[]> ExportSettlementsToExcelAsync(SettlementFilterRequest filter)
		{
			if (!filter.Month.HasValue || !filter.Year.HasValue)
			{
				throw new AppException("please filter month and year", 400);
			}

			if (filter.Month.Value < 1 || filter.Month.Value > 12)
			{
				throw new AppException("Month is from 1 - 12 ", 400);
			}

			// --------------------------------------------------

			// 3. Đã an toàn, bắt đầu đi lấy data
			var settlements = await GetAllFilterAsync(filter);

			// 4. Check rỗng
			if (settlements == null || !settlements.Any())
			{
				throw new AppException($"No data to export {filter.Month}/{filter.Year}!", 404);
			}

			// 5. Đóng gói thành Excel
			var fileBytes = _exportService.ExportSettlements(settlements);

			return fileBytes;
		}

		public async Task<bool> ConfirmReceiptAsync(Guid settlementId, ICurrentUser currentUser)
		{
			var settlement = await _unitOfWork.Repository<MonthlySettlement>().GetByIdAsync(settlementId);

			if (settlement == null)
			{
				throw new Exception("Không tìm thấy phiếu đối soát!");
			}

			if (!currentUser.PartnerId.HasValue || settlement.PartnerId != currentUser.PartnerId.Value)
			{
				throw new AppException("Bạn không có quyền truy cập hoặc xác nhận phiếu đối soát này!", 403);
			}

			if (settlement.Status != "PAID")
			{
				throw new Exception("Phiếu đối soát chưa được chuyển khoản hoặc đã được xác nhận rồi.");
			}
			settlement.Status = "RECEIVED";

			_unitOfWork.Repository<MonthlySettlement>().Update(settlement);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}

        public async Task<UnpaidWalletResponse> GetTotalPendingAmountAsync(Guid partnerId)
        {
           
            var unsettledAmount = await _commissionHistoryRepsitory.GetTotalUnsettledAmountAsync(partnerId);
            var pendingAmount = await _settlementRepository.GetTotalPendingAmountAsync(partnerId);

            return new UnpaidWalletResponse
            {
                UnsettledAmount = unsettledAmount,
                PendingSettlementAmount = pendingAmount
            };
        }

        public async Task FinalizeSettlementAsync(Guid settlementId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var settlement = await _unitOfWork.Repository<MonthlySettlement>().GetByIdAsync(settlementId);
                if (settlement == null || settlement.Status != "PENDING")
                    throw new AppException("Chỉ có thể chốt phiếu đang ở trạng thái PENDING!", 400);

           
                settlement.Status = "FINALIZED";
                _unitOfWork.Repository<MonthlySettlement>().Update(settlement);

              
                var ordersToLock = await _settlementRepository.GetOrdersBySettlementIdAsync(settlementId);

                foreach (var order in ordersToLock)
                {
                    order.IsLocked = true;
                    order.LockedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Order>().Update(order);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
