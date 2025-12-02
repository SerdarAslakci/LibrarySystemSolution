using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IFineService
    {
        Task<Fine?> ProcessLateReturnAsync(Loan loan);
        Task<IEnumerable<UserFineDto>> GetUserFinesByEmailAsync(string email);
        Task<UserFineDto?> RevokeFineAsync(int fineId);
        Task<UserFineDto> AddFineAsync(CreateFineDto fineDto);
    }
}
