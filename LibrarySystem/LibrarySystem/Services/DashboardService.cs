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

            var totalBooksTask = _bookRepository.GetBookCountAsync();
            var normalUsersTask = _userRepository.GetUserCountAsync();
            var loanedBooksTask = _loanRepository.GetLoanedBookCountAsync();
            var overdueLoansTask = _loanRepository.GetOverdueLoanCountAsync();

            try
            {
                // Task.WhenAll ile hepsinin tamamlanmasını bekliyoruz.
                // Hepsnini paralel olarak çalışmasını sağlar.
                await Task.WhenAll(totalBooksTask, normalUsersTask, loanedBooksTask, overdueLoansTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard verileri alınırken bir veya daha fazla görev başarısız oldu.");
                throw;
            }

            var dashboard = new DashboardDto
            {
                TotalBookCount = await totalBooksTask,
                UserCount = await normalUsersTask,
                LoanedBookCount = await loanedBooksTask,
                OverdueLoanCount = await overdueLoansTask
            };

            _logger.LogInformation("Dashboard verileri başarıyla alındı.");
            return dashboard;
        }
    }
}
