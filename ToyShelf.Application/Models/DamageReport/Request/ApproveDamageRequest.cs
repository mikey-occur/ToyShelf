using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.DamageReport.Request
{
	public class ApproveDamageRequest
	{
		public Guid WarehouseLocationId { get; set; } 
		public string? AdminNote { get; set; }
	}
}
