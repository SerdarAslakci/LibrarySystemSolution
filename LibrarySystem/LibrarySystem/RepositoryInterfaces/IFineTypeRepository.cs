using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IFineTypeRepository
    {
        Task<FineType> AddFineTypeAsync(FineType fineType);
        Task<FineType> UpdateFineTypeAsync(FineType fineType);
        Task<FineType?> GetByIdAsync(int id);
        Task<List<FineType>> GetAllFineTypesAsync();
    }
}
