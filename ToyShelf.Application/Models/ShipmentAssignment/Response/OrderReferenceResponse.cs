using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.ShipmentAssignment.Response
{
	public class OrderReferenceResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = string.Empty;
	}
}
