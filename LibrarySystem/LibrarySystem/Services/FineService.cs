using AutoMapper;
using LibrarySystem.API.Dtos.FineDtos;
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

        public async Task<UserFineDto> AddFineAsync(CreateFineDto fineDto)
        {
            if (fineDto == null)
            {
                _logger.LogWarning("Ceza oluşturma işlemi başarısız: Gönderilen veri boş.");
                throw new ArgumentNullException(nameof(fineDto), "Ceza verisi boş olamaz.");
            }

            _logger.LogInformation("Ceza oluşturma süreci başladı. UserId: {UserId}, Tutar: {Amount}", fineDto.userId, fineDto.amount);

            var fine = new Fine
            {
                UserId = fineDto.userId,
                FineTypeId = fineDto.fineTypeId,
                Amount = fineDto.amount,
                Description = fineDto.reason,
                IssuedDate = DateTime.Now,
                Status = "Unpaid",
                IsActive = true
            };

            var added = await _fineRepository.AddFineAsync(fine);

            _logger.LogInformation("Ceza başarıyla veritabanına kaydedildi. Yeni FineId: {Id}", fine.Id);

            return _mapper.Map<UserFineDto>(added);
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

        public async Task<UserFineDto?> RevokeFineAsync(int fineId)
        {
            _logger.LogInformation("Ceza kaldırma işlemi başlatıldı. FineId: {FineId}", fineId);

            var fine = await _fineRepository.RevokeFineByIdAscyn(fineId);

            if (fine == null)
            {
                _logger.LogWarning("Ceza kaldırma başarısız: Ceza bulunamadı. FineId: {FineId}", fineId);
                throw new KeyNotFoundException("Ceza bulunamadı.");
            }

            var userFineDto = _mapper.Map<UserFineDto>(fine);

            _logger.LogInformation("Ceza başarıyla kaldırıldı. FineId: {FineId}", fineId);

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

        public async Task<PaginatedFineResult<UserFineDto>> GetActiveFinesByUserIdAsync(string userId, FinePageableDto dto)
        {
            _logger.LogInformation(
                "Aktif cezalar için sorgu başlatıldı. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
                userId, dto.page, dto.pageSize);

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Aktif ceza sorgusu başarısız: UserId geçersiz.");
                throw new ArgumentException("UserId geçersiz.");
            }

            var result = await _fineRepository.GetActiveFinesByUserIdAsync(userId, dto.page,dto.pageSize);

            if (result == null)
            {
                _logger.LogWarning("Aktif cezalar bulunamadı veya repository null döndürdü. UserId: {UserId}", userId);
                throw new KeyNotFoundException("Kullanıcıya ait aktif ceza bulunamadı.");
            }

            _logger.LogInformation(
                "Aktif cezalar başarıyla getirildi. UserId: {UserId}, Toplam: {TotalCount}",
                userId, result.Count());

            var mapped = _mapper.Map<List<UserFineDto>>(result);

            return new PaginatedFineResult<UserFineDto>(mapped, result.Count(), dto.page, dto.pageSize);
        }


        public async Task<PaginatedFineResult<UserFineDto>> GetInActiveFinesByUserIdAsync(string userId, FinePageableDto dto)
        {
            _logger.LogInformation(
                "Pasif cezalar için sorgu başlatıldı. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
                userId, dto.page, dto.pageSize);

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Pasif ceza sorgusu başarısız: UserId geçersiz.");
                throw new ArgumentException("UserId geçersiz.");
            }

            var result = await _fineRepository.GetInActiveFinesByUserIdAsync(userId, dto.page,dto.pageSize);

            if (result == null)
            {
                _logger.LogWarning("Pasif cezalar bulunamadı veya repository null döndürdü. UserId: {UserId}", userId);
                throw new KeyNotFoundException("Kullanıcıya ait pasif ceza bulunamadı.");
            }

            _logger.LogInformation(
                "Pasif cezalar başarıyla getirildi. UserId: {UserId}, Toplam: {TotalCount}",
                userId, result.Count());

            var mapped = _mapper.Map<List<UserFineDto>>(result);

            return new PaginatedFineResult<UserFineDto>(mapped, result.Count(), dto.page, dto.pageSize);
        }

    }
}