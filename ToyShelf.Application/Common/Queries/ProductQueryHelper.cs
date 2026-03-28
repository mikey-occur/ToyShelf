using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Common.Queries
{
	public static class ProductQueryHelper
	{
		public static IQueryable<Product> ApplyFilter(
			IQueryable<Product> query,
			bool? isActive,
			Guid? categoryId,
			string? searchItem)
		{
			if (isActive.HasValue)
				query = query.Where(p => p.IsActive == isActive.Value);

			if (categoryId.HasValue)
				query = query.Where(p => p.ProductCategoryId == categoryId.Value);

			if (!string.IsNullOrWhiteSpace(searchItem))
			{
				var keyword = searchItem.Trim().ToLower();

				query = query.Where(p =>
					p.Name.ToLower().Contains(keyword) ||
					p.SKU.ToLower().Contains(keyword) ||
					(p.Barcode != null && p.Barcode.ToLower().Contains(keyword))
				);

				query = query
					.OrderByDescending(p => p.SKU.ToLower() == keyword || (p.Barcode != null && p.Barcode.ToLower() == keyword))
					.ThenByDescending(p => p.Name.ToLower().StartsWith(keyword))
					.ThenByDescending(p => p.CreatedAt);
			}
			else
			{
				query = query.OrderByDescending(p => p.CreatedAt);
			}

			return query;
		}
	}
}
