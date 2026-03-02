using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Models.City.Response
{
	public class CityResponse
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = null!;
		public string Name { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

}
