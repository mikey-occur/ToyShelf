using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Domain.Entities
{
	public class CommissionItemCategory
	{
		public Guid CommissionItemId { get; set; }
		public Guid ProductCategoryId { get; set; }
		public virtual CommissionItem CommissionItem { get; set; } = null!;
		public virtual ProductCategory ProductCategory { get; set; } = null!;

	}
}
