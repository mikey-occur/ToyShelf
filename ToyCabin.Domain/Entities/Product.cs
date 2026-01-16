using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Domain.Entities
{
	public class Product
	{
		public Guid Id { get; set; }
		public Guid ProductCategoryId { get; set; }

		public string SKU { get; set; } = string.Empty; // DUY NHẤT -> Stock Keeping Unit VD: Product là Gundam RX-78 HG thì SKU là GDM-HG-RX78
		public string Name { get; set; } = string.Empty;
		public decimal BasePrice { get; set; }

		public string? Description { get; set; }
		public string? Brand { get; set; }
		public string? Material { get; set; }
		public string? OriginCountry { get; set; }
		public string? AgeRange { get; set; }

		public string? QrCode { get; set; }
		public string? Model3DUrl { get; set; } // link to 3D model
		public string? ImageUrl { get; set; }

		public bool IsActive { get; set; } = true;
		public bool IsConsignment { get; set; } = true; // hàng ký gửi -> hàng của store

		public DateTime CreatedAt { get; set; } 
		public DateTime? UpdatedAt { get; set; }

		public virtual ProductCategory ProductCategory { get; set; } = null!;
		public virtual ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
	}
}
