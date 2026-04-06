using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.DamageReport.Request
{
	public class CreateFromDamageRequest
	{
		public Guid DamageReportId { get; set; }
		public Guid WarehouseLocationId { get; set; }
	}
}
