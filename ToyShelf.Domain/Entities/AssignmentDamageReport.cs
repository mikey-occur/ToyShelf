using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class AssignmentDamageReport
	{
		public Guid Id { get; set; }
		public Guid DamageReportId { get; set; }
		public Guid ShipmentAssignmentId { get; set; }

		public virtual DamageReport DamageReport { get; set; } = null!;
		public virtual ShipmentAssignment ShipmentAssignment { get; set; } = null!;
	}
}
