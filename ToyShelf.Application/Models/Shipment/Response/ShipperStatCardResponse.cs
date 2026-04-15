using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShipperStatCardResponse
	{
		public int TotalDelivering { get; set; } 
		public int TotalCompleted { get; set; }  
		public int TotalCancelled { get; set; }  
		public int TotalAll { get; set; }
	}
}
