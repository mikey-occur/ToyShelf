using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class ProductCategory
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = string.Empty; // ROBOT-DOG 
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public Guid? ParentId { get; set; }         
		public ProductCategory? Parent { get; set; } 
		public ICollection<ProductCategory> Children { get; set; }
			= new List<ProductCategory>();
		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public virtual ICollection<Product> Products { get; set; } = new List<Product>();	
	}
}
