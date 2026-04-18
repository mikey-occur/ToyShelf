using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Auth;
using ToyShelf.Application.Models.StoreCreationRequest.Request;
using ToyShelf.Application.Models.StoreCreationRequest.Response;
using ToyShelf.Domain.Entities;

namespace ToyShelf.Application.IServices
{
	public interface IStoreCreationRequestService
	{
		Task<StoreCreationRequestResponse> CreateAsync(CreateStoreCreationRequest request, ICurrentUser currentUser);

		Task<IEnumerable<StoreCreationRequestResponse>> GetRequestsAsync(StoreRequestStatus? status, Guid? partnerId);

		Task<StoreCreationRequestResponse> GetByIdAsync(Guid id);

		Task ReviewAsync(Guid id, ReviewStoreCreationRequest request, ICurrentUser currentUser);

		Task DeleteAsync(Guid id);
	}
}
