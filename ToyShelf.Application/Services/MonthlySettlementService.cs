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
		public MonthlySettlementService(IUnitOfWork unitOfWork, ICommissionHistoryRepsitory commissionHistoryRepsitory, IMonthlySettlementRepository settlementRepository, IExcelService exportService, INotificationService notificationService, INotificationBroadcaster notificationBroadcaster, IUserRepository userRepository)
		{
			_unitOfWork = unitOfWork;
			_commissionHistoryRepsitory = commissionHistoryRepsitory;
			_settlementRepository = settlementRepository;
			_exportService = exportService;
			_notificationService = notificationService;
			_notificationBroadcaster = notificationBroadcaster;
			_userRepository = userRepository;
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
                // Lấy ngày (cắt bỏ phần giờ phút giây) để group
                .GroupBy(ch => ch.OrderItem?.Order?.CreatedAt.Date ?? ch.CreatedAt.Date)
                .Select(group => new DailySettlementSummaryResponse
                {
                    // Format ngày ra chuỗi cho Front-end dễ hiển thị 
                    Date = group.Key.ToString("yyyy-MM-dd"),

                 
                    TotalOrders = group.Select(ch => ch.OrderItem?.OrderId).Distinct().Count(),

                   
                    TotalProductsSold = group.Sum(ch => ch.OrderItem?.Quantity ?? 0),
                    TotalSalesAmount = group.Sum(ch => ch.SalesAmount),
                    TotalCommissionAmount = group.Sum(ch => ch.CommissionAmount),

                    
                    Transactions = group.Select(ch => new CommissionHistoryResponse
                    {
                        Id = ch.Id,
                        OrderId = ch.OrderItem?.OrderId ?? Guid.Empty, // Đã lấy được OrderId
                        OrderItemId = ch.OrderItemId,
                        AppliedRate = ch.AppliedRate,
                        CommissionAmount = ch.CommissionAmount,
                        Quantity = ch.OrderItem?.Quantity ?? 0,
                        OrderCode = ch.OrderItem?.Order?.OrderCode ?? 0,
                        PaymentMethod = ch.OrderItem?.Order?.PaymentMethod ?? "Unknown",
                        OrderDate = ch.OrderItem?.Order?.CreatedAt ?? DateTime.MinValue,
                        CreatedAt = ch.CreatedAt
                    }).ToList()
                })
                .OrderByDescending(d => d.Date) 
                .ToList();

            return response;
        }

		public async Task<bool> PayAsync(Guid id)
		{
			var settlement = await _unitOfWork.Repository<MonthlySettlement>().GetByIdAsync(id);

			// Thêm check xem đã Paid hoặc đã Received chưa
			if (settlement == null || settlement.Status == "PAID" || settlement.Status == "RECEIVED")
			{
				return false;
			}

			settlement.Status = "PAID";
			settlement.PaidAt = DateTime.UtcNow;
			_unitOfWork.Repository<MonthlySettlement>().Update(settlement);

			// Lưu vào DB
			var isSaved = await _unitOfWork.SaveChangesAsync() > 0;
			var partner = await _userRepository.GetByIdAsync(settlement.PartnerId);

			if (partner == null)
			{
				// Tùy logic hệ thống, sếp có thể văng lỗi hoặc return thoát hàm luôn
				throw new AppException("Không tìm thấy thông tin Partner để gửi thông báo", 404);
			}
			// BẮN NOTIFICATION CHO PARTNER
			if (isSaved)
			{
				
				var request = new CreateNotificationRequest
				{
					UserId = partner.Id, 
					Title = "Thanh toán đối soát thành công",
					
					Content = $"Kỳ đối soát tháng này đã được chuyển khoản. Vui lòng bấm Xác Nhận khi bạn đã nhận được."
				};

				await _notificationService.CreateNotificationAsync(request);

				await _notificationBroadcaster.SendNotificationToUserAsync(
					partner.Id,
					request.Title,
					request.Content
				);
			}

			return isSaved;
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
			//Guid currentPartnerId = currentUser.UserId;

			//if (settlement.PartnerId != currentPartnerId)
			//{
			//	throw new Exception("Bạn không có quyền xác nhận phiếu đối soát này!");
			//}
			if (settlement.Status != "PAID")
			{
				throw new Exception("Phiếu đối soát chưa được chuyển khoản hoặc đã được xác nhận rồi.");
			}
			settlement.Status = "RECEIVED";

			_unitOfWork.Repository<MonthlySettlement>().Update(settlement);
			await _unitOfWork.SaveChangesAsync();

			return true;
		}


	}
}
