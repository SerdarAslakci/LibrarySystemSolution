using AutoMapper;
using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.LoanDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IBookService _bookService;
        private readonly IFineService _fineService;
        private readonly IMapper _mapper;

        public LoanService(ILoanRepository loanRepository, IBookService bookCopyService, IFineService fineService, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _bookService = bookCopyService;
            _fineService = fineService;
            _mapper = mapper;
        }

        public async Task<bool> CanUserBorrowAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId boş olamaz.", nameof(userId));

            return await _loanRepository.CanUserBarrowAsync(userId);
        }

        public async Task<LoanHistoryDto> CreateLoanAsync(string userId, CreateLoanDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId boş olamaz.", nameof(userId));

            var bookCopy = await _bookService.GetBookCopyByBarcodeAsync(dto.Barcode);

            if (bookCopy == null)
                throw new KeyNotFoundException("Barkod ile kitap kopyası bulunamadı.");

            if (!bookCopy.IsAvailable)
                throw new InvalidOperationException("Kitap şu anda ödünçte.");

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
                throw new Exception("Ödünç verme işlemi başarısız oldu.");

            await _bookService.SetBookCopyUnAvailableAsync(bookCopy.Id);

            return _mapper.Map<LoanHistoryDto>(addedLoan);
        }

        public async Task<LoanHistoryDto> UpdateLoanAsync(UpdateLoanDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var loan = await _loanRepository.GetLoanByIdAsync(dto.LoanId);
            if (loan == null)
                throw new KeyNotFoundException("Ödünç kaydı bulunamadı.");

            if (loan.ActualReturnDate != null)
                throw new InvalidOperationException("Bu ödünç işlemi zaten tamamlanmış.");

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

            return _mapper.Map<LoanHistoryDto>(updatedLoan);
        }


        public async Task<LoanHistoryDto?> GetLoanByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçersiz Loan Id.", nameof(id));

            var loan = await _loanRepository.GetLoanByIdAsync(id);

            return _mapper.Map<LoanHistoryDto>(loan);
        }

        public async Task<ReturnSummaryDto?> ReturnBookAsync(string barcode)
        {
            var loan = await _loanRepository.MarkAsReturnedByBarcodeAsync(barcode);

            if (loan == null)
                throw new KeyNotFoundException("Bu barkoda ait aktif bir ödünç işlemi bulunamadı.");

            await _fineService.ProcessLateReturnAsync(loan);

            var summaryDto = _mapper.Map<ReturnSummaryDto>(loan);

            return summaryDto;
        }

        public async Task<IEnumerable<LoanHistoryDto?>> GetAllLoansByUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Kullanıcı kimliği (userId) boş veya geçersiz olamaz.", nameof(userId));
            }

            var loans = await _loanRepository.GetAllLoansByUserAsync(userId);

            var dtos =  _mapper.Map<IEnumerable<LoanHistoryDto?>>(loans);

            return dtos;
        }
    }
}
