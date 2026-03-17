using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.ShipmentAssignment.Request;
using ToyShelf.Application.Models.ShipmentAssignment.Response;

namespace ToyShelf.Application.IServices
{
	public interface IShipmentAssignmentService
	{
		Task<ShipmentAssignmentResponse> CreateAsync(CreateShipmentAssignmentRequest request, ICurrentUser currentUser);
		Task AcceptAsync(Guid id, ICurrentUser currentUser);
		Task RejectAsync(Guid id, ICurrentUser currentUser);
		Task<IEnumerable<ShipmentAssignmentResponse>> GetMyAssignments(ICurrentUser currentUser);
		Task<IEnumerable<ShipmentAssignmentResponse>> GetByStoreOrderId(Guid storeOrderId);
	}

}
