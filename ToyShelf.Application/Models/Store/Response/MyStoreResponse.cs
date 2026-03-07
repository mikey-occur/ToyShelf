using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.Store.Response
{
	public class MyStoreResponse
	{
		public Guid StoreId { get; set; }

		public string StoreName { get; set; } = string.Empty;

		public string StoreCode { get; set; } = string.Empty;

		public StoreRole StoreRole { get; set; }
	}
}
