using ToyCabin.Application.IServices;
using ToyCabin.Application.Models.Cabin.Request;
using ToyCabin.Application.Models.Cabin.Response;
using ToyCabin.Domain.Common.Time;
using ToyCabin.Domain.Entities;
using ToyCabin.Domain.IRepositories;

namespace ToyCabin.Application.Services
{
    public class CabinService : ICabinService
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly IDateTimeProvider _dateTime;
		private readonly ICabinRepository _cabinRepository;

		public CabinService(IUnitOfWork unitOfWork, IDateTimeProvider dateTime, ICabinRepository cabinRepository)
		{
			_unitOfWork = unitOfWork;
			_dateTime = dateTime;
			_cabinRepository = cabinRepository;
		}

        public async Task<CabinResponse> CreateCabinAsync(CreateCabinRequest request)
        {
            var cabin = new Cabin
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                LocationDescription = request.LocationDescription,
                StoreId = request.StoreId,
                IsActive = request.IsActive,
                IsOnline = request.IsOnline,
                CreatedAt = _dateTime.UtcNow
            };
            await _cabinRepository.AddAsync(cabin);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(cabin);
        }

        public async Task<bool> DeleteCabinAsync(Guid cabinId)
        {
            var cabin = await _cabinRepository.GetByIdAsync(cabinId);
            if (cabin == null)
            {
                throw new Exception($"Cabin with ID {cabinId} not found.");
            }
            cabin.IsActive = false;
            _cabinRepository.Update(cabin);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CabinResponse>> GetActiveCabinsAsync()
        {
           var cabins = await _cabinRepository.FindAsync(c => c.IsActive);
           return cabins.Select(MapToResponse);
        }
        public async Task<IEnumerable<CabinResponse>> GetAllCabinsAsync()
        {
            var cabins = await _cabinRepository.GetAllAsync();
            return cabins.Select(MapToResponse);
        }

        public async Task<IEnumerable<CabinResponse>> GetAllOfflineCabinsAsync()
        {
            var cabins = await _cabinRepository.FindAsync(c => !c.IsOnline);
            return cabins.Select(MapToResponse);
        }
        

        public async Task<IEnumerable<CabinResponse>> GetAllOnlineCabinsAsync()
        {
            var cabins = await _cabinRepository.FindAsync(c => c.IsOnline);
            return cabins.Select(MapToResponse);
        }

        public async Task<CabinResponse?> GetCabinByIdAsync(Guid cabinId)
        {
           var cabin =  await _cabinRepository.GetByIdAsync(cabinId);
           return cabin == null ? null : MapToResponse(cabin);
        }

        public async Task<IEnumerable<CabinResponse>> GetInactiveCabinsAsync()
        {
            var cabins = await _cabinRepository.FindAsync(c => !c.IsActive);
            return cabins.Select(MapToResponse);
        }

        public async Task<CabinResponse> UpdateCabinAsync(Guid cabinId, UpdateCabinRequest request)
        {
            var cabin = await _cabinRepository.GetByIdAsync(cabinId);
            if (cabin == null)
            {
                throw new Exception($"Cabin with ID {cabinId} not found.");
            }

            cabin.Name = request.Name;
            cabin.Code = request.Code;
            cabin.LocationDescription = request.LocationDescription;
            cabin.StoreId = request.StoreId;
            cabin.IsActive = request.IsActive ?? cabin.IsActive;
            cabin.IsOnline = request.IsOnline ?? cabin.IsOnline;
            _cabinRepository.Update(cabin);
            await _unitOfWork.SaveChangesAsync();
            return MapToResponse(cabin);
        }

        private static CabinResponse MapToResponse(Cabin cabin)
		{
			return new CabinResponse
			{
				Id = cabin.Id,
				Name = cabin.Name,
				Code = cabin.Code,
                LocationDescription = cabin.LocationDescription,
                StoreId = cabin.StoreId,
				IsActive = cabin.IsActive,
				CreatedAt = cabin.CreatedAt,
                IsOnline = cabin.IsOnline,
                LastHeartbeatAt = cabin.LastHeartbeatAt
			};
		}
    }
}