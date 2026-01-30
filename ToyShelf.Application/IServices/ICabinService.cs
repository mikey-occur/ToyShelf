using ToyShelf.Application.Models.Cabin.Request;
using ToyShelf.Application.Models.Cabin.Response;

namespace ToyShelf.Application.IServices
{
    public interface ICabinService
    {
      Task<IEnumerable<CabinResponse>> GetAllCabinsAsync();
      Task<IEnumerable<CabinResponse>> GetActiveCabinsAsync();
      Task<IEnumerable<CabinResponse>> GetInactiveCabinsAsync();
      Task<IEnumerable<CabinResponse>> GetAllOnlineCabinsAsync();
      Task<IEnumerable<CabinResponse>> GetAllOfflineCabinsAsync();
      Task<CabinResponse> GetCabinByIdAsync(Guid cabinId);
      Task<CabinResponse> CreateCabinAsync(CreateCabinRequest request);
      Task<CabinResponse> UpdateCabinAsync(Guid cabinId, UpdateCabinRequest request);
      Task<bool> DeleteCabinAsync(Guid cabinId);
    }
}