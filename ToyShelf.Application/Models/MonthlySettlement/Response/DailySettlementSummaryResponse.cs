using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.CommissionHistory.Response;

namespace ToyShelf.Application.Models.MonthlySettlement.Response
{
    public class DailySettlementSummaryResponse
    {
        public string Date { get; set; } = string.Empty; 
        public int TotalOrders { get; set; }             
        public int TotalProductsSold { get; set; }       
        public decimal TotalSalesAmount { get; set; }    
        public decimal TotalCommissionAmount { get; set; } 
        public List<OrderTransactionResponse> Transactions { get; set; } = new();
    }
}
