using LibrarySystem.API.Dtos.DashboardDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;

namespace LibrarySystem.API.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IBookRepository bookRepository,
            IUserRepository userRepository,
            ILoanRepository loanRepository,
            ILogger<DashboardService> logger)
        {
            _bookRepository = bookRepository;
            _userRepository = userRepository;
            _loanRepository = loanRepository;
            _logger = logger;
        }


        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            _logger.LogInformation("Dashboard verileri alınmaya başlıyor.");

            var totalBooks = await _bookRepository.GetBookCountAsync();
            var normalUsers = await _userRepository.GetUserCountAsync();
            var loanedBooks = await _loanRepository.GetLoanedBookCountAsync();
            var overdueLoans = await _loanRepository.GetOverdueLoanCountAsync();

            var dashboard = new DashboardDto
            {
                TotalBookCount = totalBooks,
                UserCount = normalUsers,
                LoanedBookCount = loanedBooks,
                OverdueLoanCount = overdueLoans
            };

            _logger.LogInformation("Dashboard verileri başarıyla alındı.");

            return dashboard;
        }

    }
}
