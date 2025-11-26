using AutoMapper;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class FineService : IFineService
    {
        private readonly IFineRepository _fineRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<FineService> _logger;

        public FineService(IFineRepository fineRepository, IUserService userService, IMapper mapper, ILogger<FineService> logger)
        {
            _fineRepository = fineRepository;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<UserFineDto>> GetUserFinesByEmailAsync(string email)
        {
            var user = await _userService.GetUserDetailByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Ceza sorgulama başarısız: '{Email}' kullanıcısı bulunamadı.", email);
                throw new KeyNotFoundException($"'{email}' email adresine sahip kullanıcı bulunamadı.");
            }

            var fines = await _fineRepository.GetUserFinesWithLoanAsync(user.Id);

            return _mapper.Map<IEnumerable<UserFineDto>>(fines);
        }

        public async Task<UserFineDto?> PayFineAsync(int fineId)
        {
            _logger.LogInformation("Ceza ödeme işlemi başlatıldı. FineId: {FineId}", fineId);

            var fine = await _fineRepository.PayFineByIdAscyn(fineId);

            if (fine == null)
            {
                _logger.LogWarning("Ceza ödeme başarısız: Ceza bulunamadı. FineId: {FineId}", fineId);
                throw new KeyNotFoundException("Ceza bulunamadı.");
            }

            var userFineDto = _mapper.Map<UserFineDto>(fine);

            _logger.LogInformation("Ceza başarıyla ödendi. FineId: {FineId}", fineId);

            return userFineDto;
        }

        public async Task<Fine?> ProcessLateReturnAsync(Loan loan)
        {
            if (loan == null)
            {
                _logger.LogWarning("Ceza hesaplama başarısız: Loan nesnesi null.");
                throw new ArgumentNullException(nameof(loan), "Ceza hesaplama işlemi için gönderilen ödünç (Loan) kaydı boş olamaz.");
            }

            if (loan.ActualReturnDate == null)
            {
                _logger.LogWarning("Ceza hesaplama başarısız: İade tarihi (ActualReturnDate) yok. LoanId: {LoanId}", loan.Id);
                throw new InvalidOperationException("Kitap henüz iade edilmediği (ActualReturnDate boş olduğu) için ceza hesaplanamaz.");
            }

            if (string.IsNullOrEmpty(loan.UserId) || loan.Id <= 0)
            {
                _logger.LogWarning("Ceza hesaplama başarısız: Geçersiz LoanId ({LoanId}) veya UserId.", loan.Id);
                throw new ArgumentException("Geçersiz Loan veya User ID bilgisi.");
            }

            try
            {
                _logger.LogInformation("Gecikme cezası işlemi başlatılıyor. LoanId: {LoanId}, UserId: {UserId}", loan.Id, loan.UserId);

                var fine = await _fineRepository.ProcessLateReturnAsync(loan);

                if (fine != null)
                {
                    _logger.LogInformation("Gecikme cezası oluşturuldu/güncellendi. FineId: {FineId}, Tutar: {Amount}", fine.Id, fine.Amount);
                }
                else
                {
                    _logger.LogInformation("Gecikme cezası oluşmadı (Zamanında iade veya muafiyet). LoanId: {LoanId}", loan.Id);
                }

                return fine;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Ceza işlemi sırasında mantıksal hata. LoanId: {LoanId}", loan.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ceza işlemi sırasında beklenmeyen hata oluştu. LoanId: {LoanId}", loan.Id);
                throw new Exception("Ceza işlemi sırasında beklenmeyen bir hata oluştu. Lütfen sistem yöneticisine başvurun.", ex);
            }
        }
    }
}