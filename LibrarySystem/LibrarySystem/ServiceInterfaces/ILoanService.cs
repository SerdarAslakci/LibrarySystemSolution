using LibrarySystem.API.Dtos.LoanDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface ILoanService 
    {
        Task<bool> CanUserBorrowAsync(string userId);
        Task<LoanHistoryDto> CreateLoanAsync(string userId, CreateLoanDto dto);
        Task<LoanHistoryDto> UpdateLoanAsync(UpdateLoanDto loan);
        Task<LoanHistoryDto?> GetLoanByIdAsync(int id);
        Task<ReturnSummaryDto?> ReturnBookAsync(string barcode);
        Task<IEnumerable<LoanHistoryDto>?> GetAllLoansByUserAsync(string userId);
    }
}
