using LibrarySystem.API.Dtos.FineDtos;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IFineService
    {
        Task<PaginatedFineResult<UserFineDto>> GetActiveFinesByUserIdAsync(string userId, FinePageableDto dto);
        Task<PaginatedFineResult<UserFineDto>> GetInActiveFinesByUserIdAsync(string userId, FinePageableDto dto);
        Task<Fine?> ProcessLateReturnAsync(Loan loan);
        Task<IEnumerable<UserFineDto>> GetUserFinesByEmailAsync(string email);
        Task<UserFineDto?> RevokeFineAsync(int fineId);
        Task<UserFineDto?> PayFineAsync(string userId, int fineId);
        Task<UserFineDto> AddFineAsync(CreateFineDto fineDto);
    }
}
