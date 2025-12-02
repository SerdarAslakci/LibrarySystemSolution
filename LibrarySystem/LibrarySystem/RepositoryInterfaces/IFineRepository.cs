using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IFineRepository
    {
        Task<Fine?> ProcessLateReturnAsync(Loan loan);
        Task<IEnumerable<Fine>> GetUserFinesWithLoanAsync(string userId);
        Task<Fine?> RevokeFineByIdAscyn(int id);
        Task<Fine> AddFineAsync(Fine fine);
    }
}
