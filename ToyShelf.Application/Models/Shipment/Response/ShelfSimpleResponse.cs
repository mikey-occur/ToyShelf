using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Shipment.Response
{
	public class ShelfSimpleResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = string.Empty;
		public ShelfStatus Status { get; set; }
	}
}
