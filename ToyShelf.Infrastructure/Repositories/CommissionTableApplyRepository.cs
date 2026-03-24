using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Infrastructure.Repositories
{
	public class CommissionTableApplyRepository : GenericRepository<CommissionTableApply>, ICommissionTableApplyRepository
	{
		public CommissionTableApplyRepository(Context.ToyShelfDbContext context) : base(context)
		{
		}

		public async Task<CommissionTableApply?> GetActiveByPartnerAsync(Guid partnerId, DateTime now)
		{
				return await _context.CommissionTableApplies
			.Include(a => a.CommissionTable) // Load thông tin bảng giá để lấy tên/loại
			.Where(a => a.PartnerId == partnerId &&
						a.IsActive &&
						a.StartDate <= now &&
						(a.EndDate == null || a.EndDate >= now))
			.OrderByDescending(a => a.StartDate) // Nếu có nhiều bảng giá trùng lặp, lấy bảng mới nhất
			.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<CommissionTableApply>> GetAllWithDetailsAsync(bool? isActive)
		{
			var query = _context.CommissionTableApplies
				.Include(x => x.Partner)
				.Include(x => x.CommissionTable)
				.AsQueryable();

			if (isActive.HasValue)
			{
				query = query.Where(x => x.IsActive == isActive.Value);
			}

			return await query
				.OrderByDescending(x => x.StartDate)
				.ToListAsync();
		}

		public async Task<bool> HasOverlapAsync(Guid partnerId, DateTime startDate, DateTime? endDate)
		{
		
			var checkEndDate = endDate ?? DateTime.MaxValue;

			
			return await _context.CommissionTableApplies.AnyAsync(x =>
				x.PartnerId == partnerId &&
				x.IsActive &&
				x.StartDate < checkEndDate &&
				(x.EndDate ?? DateTime.MaxValue) > startDate
			);
		}
	}
}
