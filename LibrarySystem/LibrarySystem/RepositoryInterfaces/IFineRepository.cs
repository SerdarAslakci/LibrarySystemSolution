using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IFineRepository
    {
        Task<Fine?> ProcessLateReturnAsync(Loan loan);
        Task<IEnumerable<Fine>> GetUserFinesWithLoanAsync(string userId);
        Task<Fine?> PayFineByIdAscyn(int id);
    }
}
