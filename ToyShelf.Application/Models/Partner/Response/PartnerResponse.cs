using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.PriceTableApply.Response;

namespace ToyShelf.Application.Models.Partner.Response
{
	public class PartnerResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = string.Empty;
		public string CompanyName { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }

		public Guid PartnerTierId { get; set; }
		public string PartnerTierName { get; set; } = string.Empty;
		public int PartnerTierPriority { get; set; }
		public bool IsActive { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}


	public class PartnerDetailResponse : PartnerResponse
	{
		// Kế thừa toàn bộ trường ở trên, chỉ nhét thêm tài khoản vào đây
		public AppliedCommissionTableResponse? CurrentCommission { get; set; }
		public List<AppliedCommissionTableResponse> CommissionHistories { get; set; } = new();
		public PartnerAdminResponse? PartnerAccount { get; set; }
	}

	public class PartnerAdminResponse
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string? AvatarUrl { get; set; }
		public bool IsActive { get; set; }
		public DateTime? LastLoginAt { get; set; }
	}

	public class AppliedCommissionTableResponse
	{
		public Guid CommissionTableId { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}

	public class PartnerCreateResponse : PartnerResponse
	{
		public AppliedCommissionTableResponse? AppliedCommission { get; set; }
	}
}
