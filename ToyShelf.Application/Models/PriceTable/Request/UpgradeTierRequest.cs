using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.PriceTable.Request
{
	public class UpgradeTierRequest
	{
		public Guid PartnerId { get; set; }
		public Guid NewTierId { get; set; }
	}
}
