using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.InventoryDisposition.Request
{
	public class UpdateInventoryDispositionRequest
	{
		public string Code { get; set; } = null!;
		public string? Description { get; set; }
	}
}
