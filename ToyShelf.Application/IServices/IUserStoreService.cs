using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.Models.Store.Response;
using ToyShelf.Application.Models.UserStore.Request;

namespace ToyShelf.Application.IServices
{
	public interface IUserStoreService
	{
		Task<IEnumerable<MyStoreResponse>> GetMyStoresAsync(Guid userId);
	}
}
