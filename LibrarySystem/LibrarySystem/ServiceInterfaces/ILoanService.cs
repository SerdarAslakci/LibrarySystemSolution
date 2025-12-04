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
        Task<ReturnSummaryDto?> ReturnBookAsync(string userId, string barcode);
        Task<PaginatedLoanDto<LoanWithUserDetailsDto>> GetAllLoansWithUserDetailAsync(LoanPageableRequestDto loanPageableRequestDto);
        Task<PaginatedLoanDto<LoanWithUserDetailsDto>> GetAllOverdueLoansWithUserDetailAsync(LoanPageableRequestDto loanPageableRequestDto);
        Task<PaginatedLoanDto<LoanWithUserDetailsDto>> GetAllReturnedLoansWithUserDetailAsync(LoanPageableRequestDto loanPageableRequestDto);
        Task<PaginatedLoanDto<LoanHistoryDto>?> GetAllActiveLoansByUserAsync(string userId,LoanPageableRequestDto loanPageableRequestDto);
        Task<PaginatedLoanDto<LoanHistoryDto>?> GetAllReturnedLoansByUserAsync(string userId,LoanPageableRequestDto loanPageableRequestDto);
    }
}
