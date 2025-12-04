using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.LoanDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoanController> _logger;

        public LoanController(ILoanService loanService, ILogger<LoanController> logger)
        {
            _loanService = loanService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("my-active-loans")]
        public async Task<IActionResult> GetMyActiveLoans([FromQuery] LoanPageableRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Kullanıcı geçmiş ödünçleri sorgusu: Kimlik doğrulama başarısız (Claim eksik).");
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            try
            {
                var loans = await _loanService.GetAllActiveLoansByUserAsync(userId,dto);
                return Ok(loans);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Kullanıcı ödünçleri sorgusu hatası (Argüman): {Message}", ex.Message);
                return BadRequest(ex.Message );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı ödünçleri alınırken sunucu hatası. UserID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ödünç kayıtları alınırken beklenmedik bir hata oluştu.");
            }
        }


        [Authorize]
        [HttpGet("my-returned-loans")]
        public async Task<IActionResult> GetMyReturnedLoans([FromQuery] LoanPageableRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Kullanıcı geçmiş ödünçleri sorgusu: Kimlik doğrulama başarısız (Claim eksik).");
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            try
            {
                var loans = await _loanService.GetAllReturnedLoansByUserAsync(userId, dto);
                return Ok(loans);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Kullanıcı ödünçleri sorgusu hatası (Argüman): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı ödünçleri alınırken sunucu hatası. UserID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ödünç kayıtları alınırken beklenmedik bir hata oluştu.");
            }
        }

        [Authorize]
        [HttpGet("can-borrow")]
        public async Task<ActionResult<bool>> CanUserBorrow()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Ödünç alma uygunluk kontrolü: Kullanıcı bulunamadı.");
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");
            }

            try
            {
                var result = await _loanService.CanUserBorrowAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uygunluk kontrolü sırasında sunucu hatası. UserID: {UserId}", userId);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Loan>> CreateLoan([FromBody] CreateLoanDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Yeni ödünç isteği validasyon hatası.");
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Yeni ödünç isteği: Kullanıcı bulunamadı.");
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");
            }

            _logger.LogInformation("Yeni ödünç alma isteği. UserID: {UserId}, Barkod: {Barcode}, Süre: {Days} gün", userId, dto.Barcode, dto.LoanDays);

            try
            {
                var loan = await _loanService.CreateLoanAsync(userId, dto);

                _logger.LogInformation("Ödünç kaydı oluşturuldu. LoanID: {LoanId}", loan.LoanId);

                return CreatedAtAction(nameof(GetLoanById), new { id = loan.LoanId }, loan);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Ödünç alma başarısız (İş Kuralı): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödünç alma işlemi sırasında sunucu hatası.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-all-loans")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllLoansForAdminPanel([FromQuery] LoanPageableRequestDto requestDto)
        {
            _logger.LogInformation("Admin için tüm ödünçler sorgusu: Page {Page}, PageSize {PageSize}",
                requestDto.page, requestDto.pageSize);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin için tüm ödünçler sorgusu: Geçersiz istek parametreleri.");
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _loanService.GetAllLoansWithUserDetailAsync(requestDto);

                if (result.Items == null || !result.Items.Any())
                {
                    _logger.LogInformation("Admin için ödünç listesi boş döndü. Page: {Page}, PageSize: {PageSize}",
                        requestDto.page, requestDto.pageSize);
                }

                _logger.LogInformation("Admin için ödünç listesi başarıyla döndü. Count: {Count}", result.Items.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin için ödünç listesi alınırken bir hata oluştu.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ödünç listesi alınırken bir hata oluştu.");
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoanById(int id)
        {
            if (id <= 0)
                return BadRequest("Geçersiz Loan ID.");

            try
            {
                var loan = await _loanService.GetLoanByIdAsync(id);
                if (loan == null)
                {
                    _logger.LogWarning("Ödünç detayı sorgulama: Loan ID {LoanId} bulunamadı.", id);
                    return NotFound($"Loan ID {id} bulunamadı.");
                }

                return Ok(loan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödünç detayı getirilirken hata oluştu. ID: {LoanId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update-loan")]
        public async Task<ActionResult<Loan>> UpdateLoan([FromBody] UpdateLoanDto dto)
        {
            _logger.LogInformation("Ödünç güncelleme/uzatma isteği. LoanID: {LoanId}, Yeni Tarih: {NewDate}", dto?.LoanId, dto?.NewExpectedReturnDate);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Ödünç güncelleme isteği: Validasyon hatası.");
                return BadRequest(ModelState);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Kullanıcı bilgisi bulunamadı.");

                var updatedLoan = await _loanService.UpdateLoanAsync(dto);

                _logger.LogInformation("Ödünç süresi güncellendi. LoanID: {LoanId}", dto.LoanId);

                return Ok(updatedLoan);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ödünç güncelleme hatası (Bulunamadı): {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Ödünç güncelleme hatası (Geçersiz İşlem): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödünç güncelleme sırasında sunucu hatası.");
                return StatusCode(500, ex.Message);
            }
        }


        [Authorize]
        [HttpPost("return-book")]
        public async Task<ActionResult<Loan>> ReturnBook([FromBody] ReturnBookDto returnBookDto)
        {

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Kitap iade isteği: Validasyon hatası.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(returnBookDto.Barcode))
                return BadRequest("Barkod numarası gereklidir.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Kullanıcı bilgisi bulunamadı.");

            try
            {
                var loanSummaryDto = await _loanService.ReturnBookAsync(userId,returnBookDto.Barcode);

                _logger.LogInformation("Kitap başarıyla iade edildi. LoanID: {LoanId}, Gecikme Cezası: {Penalty}", loanSummaryDto.LoanId, loanSummaryDto.ReturnStatus);

                return Ok(loanSummaryDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("İade hatası (Bulunamadı): {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex,
                    "Yetkisiz erişim denemesi. UserId: {UserId}, Barcode: {Barcode}",
                    userId, returnBookDto.Barcode);

                return StatusCode(403, "Bu kitap sizin hesabınız üzerinden ödünç alınmamış. Başka bir kullanıcıya ait ödüncü iade edemezsiniz.");
            }

            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("İade hatası (Geçersiz İşlem): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İade işlemi sırasında sunucu hatası. Barkod: {Barcode}", returnBookDto.Barcode);
                return StatusCode(500, new { message = "İade işlemi sırasında beklenmeyen bir hata oluştu.", details = ex.Message });
            }

        }


        [HttpGet("overdue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOverdueLoansForAdminPanel([FromQuery] LoanPageableRequestDto request)
        {
            _logger.LogInformation("Gecikmiş ödünç kayıtları sorgusu: Sayfa {Page}, Boyut {PageSize}", request.page, request.pageSize);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Gecikmiş ödünç kayıtları sorgusu: Geçersiz istek parametreleri.");
                return BadRequest(ModelState);
            }


            try
            {
                var result = await _loanService.GetAllOverdueLoansWithUserDetailAsync(request);

                if (!result.Items.Any())
                {
                    _logger.LogInformation(
                        "Gecikmiş ödünç kaydı bulunamadı. Sayfa: {Page}, Boyut: {PageSize}",
                        request.page, request.pageSize
                    );
                }

                _logger.LogInformation(
                    "Gecikmiş ödünç kayıtları getirildi. Kayıt Sayısı: {Count}, Sayfa: {Page}, Boyut: {PageSize}",
                    result.Items.Count, request.page, request.pageSize
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Gecikmiş ödünç kayıtları getirilirken hata oluştu. Sayfa: {Page}, Boyut: {PageSize}",
                    request.page, request.pageSize
                );

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Gecikmiş ödünçler alınırken bir hata oluştu.");
            }
        }

        [HttpGet("returned")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReturnedLoansForAdminPanel([FromQuery] LoanPageableRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Teslim edilmiş ödünç kayıtları sorgusu: Geçersiz istek parametreleri.");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Teslim edilmiş ödünç kayıtları sorgusu: Sayfa {Page}, Boyut {PageSize}", request.page, request.pageSize);

            try
            {
                var result = await _loanService.GetAllReturnedLoansWithUserDetailAsync(request);

                if (!result.Items.Any())
                {
                    _logger.LogInformation(
                        "Teslim edilmiş ödünç kaydı bulunamadı. Sayfa: {Page}, Boyut: {PageSize}",
                        request.page, request.pageSize
                    );
                }

                _logger.LogInformation(
                    "Teslim edilmiş ödünç kayıtları getirildi. Kayıt Sayısı: {Count}, Sayfa: {Page}, Boyut: {PageSize}",
                    result.Items.Count, request.page, request.pageSize
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Teslim edilmiş ödünç kayıtları getirilirken hata oluştu. Sayfa: {Page}, Boyut: {PageSize}",
                    request.page, request.pageSize
                );

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ödünç geçmişi alınırken bir hata oluştu.");
            }
        }

    }
}