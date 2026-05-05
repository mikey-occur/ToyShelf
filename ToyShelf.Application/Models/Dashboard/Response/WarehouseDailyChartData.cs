using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
    public class WarehouseDailyChartData
    {
        public DateTime Date { get; set; }
        public int ShipmentProduct { get; set; }  
        public int ShipmentShelf { get; set; }    
        public int ReturnProduct { get; set; }    
        public int ReturnShelf { get; set; }
    }
}
