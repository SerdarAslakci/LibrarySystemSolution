using LibrarySystem.API.Dtos.FineTypeDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IFineTypeService
    {
        Task<ReturnFineTypeDto> AddFineTypeAsync(CreateFineTypeDto fineTypeDto);
        Task<ReturnFineTypeDto> UpdateFineTypeAsync(UpdateFineTypeDto fineType);
        Task<ReturnFineTypeDto?> GetByIdAsync(int id);
        Task<List<ReturnFineTypeDto>> GetAllFineTypesAsync();
    }
}
