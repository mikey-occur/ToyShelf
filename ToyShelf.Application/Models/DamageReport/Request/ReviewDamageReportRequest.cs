using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.Models.DamageReport.Request
{
	public class ReviewDamageReportRequest
	{
		public DamageStatus Status { get; set; } // Approved hoặc Rejected
		public string? AdminNote { get; set; }
	}
}
