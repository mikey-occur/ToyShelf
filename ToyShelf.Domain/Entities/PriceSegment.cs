using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class PriceSegment
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!; // CHEAP, MID, HIGH
		public string Name { get; set; } = null!;
		public decimal MinPrice { get; set; }
		public decimal? MaxPrice { get; set; }

		public virtual ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
		public virtual ICollection<PriceItem> PriceItems { get; set; } = new List<PriceItem>();
		public virtual ICollection<CommissionPolicy> CommissionPolicies { get; set; }
			= new List<CommissionPolicy>();
	}

}
