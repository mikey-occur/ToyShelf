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
		private readonly IUnitOfWork _unitOfWork;

		public UserStoreService(IUserStoreRepository userStoreRepository, IUnitOfWork unitOfWork)
		{
			_userStoreRepository = userStoreRepository;
			_unitOfWork = unitOfWork;
		}
		
	}
}
