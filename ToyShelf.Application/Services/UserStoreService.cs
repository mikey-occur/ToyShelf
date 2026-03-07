using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Application.Models.Store.Response;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class UserStoreService : IUserStoreService
	{
		private readonly IUserStoreRepository _userStoreRepository;

		public UserStoreService(IUserStoreRepository userStoreRepository)
		{
			_userStoreRepository = userStoreRepository;
		}

		public async Task<IEnumerable<MyStoreResponse>> GetMyStoresAsync(Guid userId)
		{
			var userStores = await _userStoreRepository
				.GetUserStoresWithStoreAsync(userId);

			return userStores.Select(x => new MyStoreResponse
			{
				StoreId = x.StoreId,
				StoreName = x.Store.Name,
				StoreCode = x.Store.Code,
				StoreRole = x.StoreRole
			});
		}
	}
}
