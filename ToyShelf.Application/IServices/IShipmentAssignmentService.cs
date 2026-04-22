using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.Shipment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IShipmentAssignmentService
	{
		Task<ShipmentAssignmentResponse> CreateAsync(CreateShipmentAssignmentRequest request, ICurrentUser currentUser);
		Task CreateFromDamageReportAsync(Guid damageReportId, Guid warehouseLocationId, ICurrentUser currentUser);
		Task AssignShipperAsync(AssignShipperRequest request, ICurrentUser currentUser);
		Task AcceptAsync(Guid id, ICurrentUser currentUser);
		Task RejectAsync(Guid id, ICurrentUser currentUser);
		Task<IEnumerable<ShipmentAssignmentResponse>> GetMyAssignments(
				ICurrentUser currentUser,
				AssignmentType? type,
				AssignmentStatus? status);
		Task<IEnumerable<ShipmentAssignmentResponse>> GetByStoreOrderId(Guid storeOrderId);
		Task<IEnumerable<ShipmentAssignmentResponse>> GetByShelfOrderId(Guid shelfOrderId);
		Task<IEnumerable<ShipmentAssignmentResponse>> GetByDamageReportId(Guid damageReportId);
		Task<IEnumerable<ShipmentAssignmentResponse>> GetAllAsync(
			AssignmentType? type,
			AssignmentStatus? status);
		Task<ShipmentAssignmentResponse> GetByIdAsync(Guid id);
	}
}
