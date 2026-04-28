using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.MonthlySettlement.Response
{
    public class UnpaidWalletResponse
    {
        public decimal UnsettledAmount { get; set; }
        public decimal PendingSettlementAmount { get; set; }
    }
}
