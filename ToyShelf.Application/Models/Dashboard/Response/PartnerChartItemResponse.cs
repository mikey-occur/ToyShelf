using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Dashboard.Response
{
	public class PartnerChartItemResponse
	{
		public string DateLabel { get; set; } = string.Empty; 
		public decimal Revenue { get; set; }                  
		public int Orders { get; set; }                     
		public decimal Commission { get; set; }
	}
}
