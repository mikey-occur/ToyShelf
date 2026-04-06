using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Application.IServices;
using ToyShelf.Domain.Common.Time;
using ToyShelf.Domain.IRepositories;

namespace ToyShelf.Application.Services
{
	public class DamageReportService : IDamageReportService
	{
		private readonly IDamageReportRepository _repository;
		private readonly IInventoryRepository _inventoryRepository;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;

		public DamageReportService(
			IDamageReportRepository repository, 
			IInventoryRepository inventoryRepository, 
			IUnitOfWork unitOfWork, 
			IDateTimeProvider dateTime)
		{
			_repository = repository;
			_inventoryRepository = inventoryRepository;
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
		}


	}
}
