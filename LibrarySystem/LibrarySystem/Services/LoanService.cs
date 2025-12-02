using AutoMapper;
using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.LoanDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IBookService _bookService;
        private readonly IFineService _fineService;
        private readonly IMapper _mapper;
        private readonly ILogger<LoanService> _logger;

        public LoanService(ILoanRepository loanRepository, IBookService bookCopyService, IFineService fineService, IMapper mapper, ILogger<LoanService> logger)
        {
            _loanRepository = loanRepository;
            _bookService = bookCopyService;
            _fineService = fineService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> CanUserBorrowAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId boş olamaz.", nameof(userId));

            return await _loanRepository.CanUserBarrowAsync(userId);
        }

        public async Task<LoanHistoryDto> CreateLoanAsync(string userId, CreateLoanDto dto)
        {
            _logger.LogInformation("Ödünç alma işlemi başlatıldı. UserId: {UserId}, Barkod: {Barcode}", userId, dto?.Barcode);

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId boş olamaz.", nameof(userId));

            var bookCopy = await _bookService.GetBookCopyByBarcodeAsync(dto.Barcode);

            if (bookCopy == null)
            {
                _logger.LogWarning("Ödünç alma başarısız: Barkod ({Barcode}) ile kitap kopyası bulunamadı.", dto.Barcode);
                throw new KeyNotFoundException("Barkod ile kitap kopyası bulunamadı.");
            }

            if (!bookCopy.IsAvailable)
            {
                _logger.LogWarning("Ödünç alma başarısız: Kitap şu an müsait değil. CopyId: {CopyId}", bookCopy.Id);
                throw new InvalidOperationException("Kitap şu anda ödünçte.");
            }

            if (dto.LoanDays <= 0)
                throw new ArgumentException("Ödünç gün sayısı sıfırdan büyük olmalıdır.", nameof(dto.LoanDays));

            var expectedReturnDate = DateTime.UtcNow.Date.AddDays(dto.LoanDays);

            if (expectedReturnDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Hesaplanan beklenen iade tarihi bugünden küçük olamaz. Gün sayısını kontrol edin.", nameof(dto.LoanDays));

            var loan = new Loan
            {
                UserId = userId,
                BookCopyId = bookCopy.Id,
                LoanDate = DateTime.UtcNow,
                ExpectedReturnDate = expectedReturnDate
            };

            var addedLoan = await _loanRepository.AddLoanAsync(loan);

            if (addedLoan == null)
            {
                _logger.LogError("Ödünç alma hatası: Veritabanına kayıt eklenemedi. UserId: {UserId}, CopyId: {CopyId}", userId, bookCopy.Id);
                throw new Exception("Ödünç verme işlemi başarısız oldu.");
            }

            await _bookService.SetBookCopyUnAvailableAsync(bookCopy.Id);

            _logger.LogInformation("Ödünç işlemi başarıyla tamamlandı. LoanId: {LoanId}", addedLoan.Id);

            return _mapper.Map<LoanHistoryDto>(addedLoan);
        }

        public async Task<LoanHistoryDto> UpdateLoanAsync(UpdateLoanDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var loan = await _loanRepository.GetLoanByIdAsync(dto.LoanId);
            if (loan == null)
            {
                _logger.LogWarning("Ödünç güncelleme başarısız: LoanId {LoanId} bulunamadı.", dto.LoanId);
                throw new KeyNotFoundException("Ödünç kaydı bulunamadı.");
            }

            if (loan.ActualReturnDate != null)
            {
                _logger.LogWarning("Ödünç güncelleme başarısız: İşlem zaten tamamlanmış (İade edilmiş). LoanId: {LoanId}", dto.LoanId);
                throw new InvalidOperationException("Bu ödünç işlemi zaten tamamlanmış.");
            }

            if (dto.NewExpectedReturnDate.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Yeni beklenen iade tarihi, **bugünün tarihinden** küçük olamaz.");
            }

            if (dto.NewExpectedReturnDate.Date < loan.ExpectedReturnDate.Date)
            {
                throw new InvalidOperationException($"Yeni beklenen iade tarihi ({dto.NewExpectedReturnDate.ToShortDateString()}), mevcut beklenen iade tarihinden ({loan.ExpectedReturnDate.ToShortDateString()}) küçük olamaz. Uzatma sadece ileri tarihe yapılabilir.");
            }

            loan.ExpectedReturnDate = dto.NewExpectedReturnDate;

            var updatedLoan = await _loanRepository.UpdateLoanAsync(loan);

            _logger.LogInformation("Ödünç süresi uzatıldı. LoanId: {LoanId}, Yeni Tarih: {NewDate}", updatedLoan.Id, updatedLoan.ExpectedReturnDate);

            return _mapper.Map<LoanHistoryDto>(updatedLoan);
        }


        public async Task<LoanHistoryDto?> GetLoanByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçersiz Loan Id.", nameof(id));

            var loan = await _loanRepository.GetLoanByIdAsync(id);

            if (loan == null)
            {
                _logger.LogWarning("Ödünç sorgulama: LoanId {LoanId} bulunamadı.", id);
            }

            return _mapper.Map<LoanHistoryDto>(loan);
        }

        public async Task<ReturnSummaryDto?> ReturnBookAsync(string barcode)
        {
            _logger.LogInformation("İade işlemi başlatıldı. Barkod: {Barcode}", barcode);

            var loan = await _loanRepository.MarkAsReturnedByBarcodeAsync(barcode);

            if (loan == null)
            {
                _logger.LogWarning("İade başarısız: Bu barkoda ({Barcode}) ait aktif ödünç bulunamadı.", barcode);
                throw new KeyNotFoundException("Bu barkoda ait aktif bir ödünç işlemi bulunamadı.");
            }

            await _fineService.ProcessLateReturnAsync(loan);

            var summaryDto = _mapper.Map<ReturnSummaryDto>(loan);

            _logger.LogInformation("İade işlemi tamamlandı. LoanId: {LoanId}, Gecikme Var Mı: {HasPenalty}", loan.Id, summaryDto.ReturnStatus);

            return summaryDto;
        }

        public async Task<IEnumerable<LoanHistoryDto?>> GetAllLoansByUserAsync(string userId)
        {

            _logger.LogInformation("Kullanıcı ödünç geçmişi sorgulanıyor. UserId: {UserId}", userId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Kullanıcı kimliği (userId) boş veya geçersiz olamaz.", nameof(userId));
            }

            var loans = await _loanRepository.GetAllLoansByUserAsync(userId);

            _logger.LogInformation("Kullanıcı ödünç geçmişi alındı. UserId: {UserId}, Kayıt Sayısı: {LoanCount}", userId, loans.Count());

            var dtos = _mapper.Map<IEnumerable<LoanHistoryDto?>>(loans);

            return dtos;
        }

        public async Task<PaginatedLoanDto<LoanWithUserDetailsDto>> GetAllLoansWithUserDetailAsync(LoanPageableRequestDto loanPageableRequestDto)
        {
            int page = loanPageableRequestDto.page;
            int pageSize = loanPageableRequestDto.pageSize;

            var loans = await _loanRepository.GetAllLoansWithUserDetail(page, pageSize);

            if (loans == null || !loans.Any())
            {
                _logger.LogWarning("Sayfalı Loan listesi boş veya null geldi. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            }

            _logger.LogInformation("Sayfalı Loan listesi başarıyla çekildi. Page: {Page}, PageSize: {PageSize}, Count: {Count}",
                page, pageSize, loans.Count());

            var loanDtos = _mapper.Map<List<LoanWithUserDetailsDto>>(loans);

            _logger.LogInformation("Loan listesi DTO'ya başarıyla dönüştürüldü. Count: {Count}", loanDtos.Count);

            int totalCount = loanDtos.Count;

            return new PaginatedLoanDto<LoanWithUserDetailsDto>(
                loanDtos,
                totalCount,
                page,
                pageSize
            );
        }

        public async Task<PaginatedLoanDto<LoanWithUserDetailsDto>> GetAllOverdueLoansWithUserDetailAsync(LoanPageableRequestDto request)
        {

            _logger.LogInformation("Gecikmiş Loanlar sorgulanıyor. Page: {Page}, PageSize: {PageSize}", request.page, request.pageSize);

            int page = request.page;
            int pageSize = request.pageSize;

            var loans = await _loanRepository.GetAllOverdueLoansWithUserDetailAsync(page, pageSize);

            _logger.LogInformation("Gecikmiş Loanlar getirildi. Page: {Page}, Size: {PageSize}, Count: {Count}",
                page, pageSize, loans.Count());

            var dtoList = _mapper.Map<List<LoanWithUserDetailsDto>>(loans);

            return new PaginatedLoanDto<LoanWithUserDetailsDto>(
                dtoList,
                dtoList.Count,
                page,
                pageSize
            );
        }

        public async Task<PaginatedLoanDto<LoanWithUserDetailsDto>> GetAllReturnedLoansWithUserDetailAsync(LoanPageableRequestDto request)
        {
            _logger.LogInformation("Tüm iade edilen Loan geçmişi sorgulanıyor. Page: {Page}, PageSize: {PageSize}", request.page, request.pageSize);

            int page = request.page;
            int pageSize = request.pageSize;

            var loans = await _loanRepository.GetAllReturnedLoansWithUserDetailAsync(page, pageSize);

            _logger.LogInformation("Returned loan getirildi. Page: {Page}, Size: {PageSize}, Count: {Count}",
                page, pageSize, loans.Count());

            var dtoList = _mapper.Map<List<LoanWithUserDetailsDto>>(loans);

            return new PaginatedLoanDto<LoanWithUserDetailsDto>(
                dtoList,
                dtoList.Count,
                page,
                pageSize
            );
        }

    }
}