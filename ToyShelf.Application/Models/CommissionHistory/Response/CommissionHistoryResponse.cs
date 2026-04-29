using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.CommissionHistory.Response
{
    public class CommissionHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public long OrderCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalCommission { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
