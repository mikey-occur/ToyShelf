using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.DamageReport.Request;
using ToyShelf.Application.Models.DamageReport.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IDamageReportService
	{
		Task<DamageReportResponse> CreateAsync(CreateDamageReportRequest request, ICurrentUser currentUser);
		Task<IEnumerable<DamageReportResponse>> GetAllAsync(DamageStatus? status);
		Task<DamageReportResponse> GetByIdAsync(Guid id);
		Task PartnerApproveAsync(Guid id, ICurrentUser currentUser);
		Task ApproveAsync(Guid id, string? adminNote, ICurrentUser currentUser);
		Task CreateRecallAssignmentAsync(Guid id, Guid warehouseLocationId, ICurrentUser currentUser);
		Task RejectAsync(Guid id, string? adminNote, ICurrentUser currentUser);
	}
}
