using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IFineRepository
    {
        Task<IEnumerable<Fine>> GetActiveFinesByUserIdAsync(string userId, int page, int pageSize);
        Task<IEnumerable<Fine>> GetInActiveFinesByUserIdAsync(string userId, int page, int pageSize);
        Task<Fine?> ProcessLateReturnAsync(Loan loan);
        Task<IEnumerable<Fine>> GetUserFinesWithLoanAsync(string userId);
        Task<Fine?> RevokeFineByIdAscyn(int id);
        Task<Fine> AddFineAsync(Fine fine);
    }
}
