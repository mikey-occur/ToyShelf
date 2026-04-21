using System;
using System.Collections.Generic;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.DamageReport.Response
{
	public class DamageReportResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = string.Empty;

		public List<Guid> ShipmentAssignmentIds { get; set; } = new List<Guid>();

		public DamageReportType Type { get; set; }
		public DamageSource Source { get; set; }
		public DamageStatus Status { get; set; }

		// Thông tin địa điểm
		public string StoreName { get; set; } = string.Empty;
		public string StoreAddress { get; set; } = string.Empty;

		// Nội dung mô tả chung
		public string? Description { get; set; }
		public string? AdminNote { get; set; }
		public bool IsWarrantyClaim { get; set; }

		// Thông tin nhân sự (Đúng yêu cầu của bồ)
		public Guid ReportedByUserId { get; set; }
		public string ReportedByName { get; set; } = string.Empty;

		public Guid? PartnerAdminApprovedByUserId { get; set; }
		public string PartnerAdminName { get; set; } = string.Empty;

		public Guid? ReviewedByUserId { get; set; }
		public string ReviewedByName { get; set; } = string.Empty;

		// Thời gian
		public DateTime CreatedAt { get; set; }
		public DateTime? PartnerAdminApprovedAt { get; set; }
		public DateTime? ReviewedAt { get; set; }

		// Danh sách chi tiết các món hỏng (1-N)
		//public List<DamageItemResponse> Items { get; set; } = new List<DamageItemResponse>();
		public List<DamageItemResponse> Items { get; set; } = new();
	}

	public class DamageItemResponse
	{
		public Guid Id { get; set; }
		public DamageItemType Type { get; set; }
		public int? Quantity { get; set; }

		public ProductInfo? Product { get; set; }
		public ShelfInfo? Shelf { get; set; }

		public List<string> MediaUrls { get; set; } = new();
	}

	public class ProductInfo
	{
		public Guid? ProductColorId { get; set; }
		public string? ProductName { get; set; }
		public string? SKU { get; set; }
		public string? ColorName { get; set; }
		public string? ImageUrl { get; set; }
	}

	public class ShelfInfo
	{
		public Guid? ShelfId { get; set; }
		public string? ShelfCode { get; set; }
	}
}