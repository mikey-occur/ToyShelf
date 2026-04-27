using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Order
{
    public class PartnerOrderDetailResponse
    {
        public Guid Id { get; set; }
        public string PartnerName { get; set; }
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string StaffEmail { get; set; } = string.Empty;
        public long OrderCode { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? BankReference { get; set; }
        public decimal TotalAmount { get; set; } // Tổng tiền khách trả
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? StoreName { get; set; }

        // THÔNG TIN MỚI DÀNH CHO PARTNER
        public decimal TotalCommission { get; set; }
        public List<PartnerOrderItemDetailResponse> Items { get; set; } = new();

        public class PartnerOrderItemDetailResponse
        {
            public Guid ProductColorId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string Sku { get; set; } = string.Empty;
            public string? ImageUrl { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal SubTotal => Price * Quantity;
            public decimal CommissionRate { get; set; }
            public decimal CommissionAmount { get; set; }
        }
    }
}
