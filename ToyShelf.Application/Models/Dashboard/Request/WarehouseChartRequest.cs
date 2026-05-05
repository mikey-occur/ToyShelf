using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Request
{
    public class WarehouseChartRequest
    {
        public string ViewType { get; set; } = "month";
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
