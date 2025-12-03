using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.API.Dtos.LoanDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface ILoanRepository
    {
        Task<Loan> AddLoanAsync(Loan loan);
        Task<Loan?> UpdateLoanAsync(Loan loan);
        Task<Loan?> GetLoanByIdAsync(int id);

        //For admin panel
        Task<IEnumerable<Loan>> GetAllLoansWithUserDetail(int page, int pageSize);
        Task<IEnumerable<Loan>> GetAllOverdueLoansWithUserDetailAsync(int page, int pageSize);
        Task<IEnumerable<Loan>> GetAllReturnedLoansWithUserDetailAsync(int page, int pageSize);


        Task<IEnumerable<Loan?>> GetAllActiveLoansByUserAsync(string userId, int page, int pageSize);
        Task<IEnumerable<Loan?>> GetAllReturnedLoansByUserAsync(string userId, int page, int pageSize);
        Task<bool> IsBookCopyOnLoanAsync(int bookCopyId);
        Task<bool> CanUserBarrowAsync(string userId);
        Task<Loan?> MarkAsReturnedByBarcodeAsync(string barcode);
        Task<int> GetLoanedBookCountAsync();
        Task<int> GetOverdueLoanCountAsync();
    }
}
