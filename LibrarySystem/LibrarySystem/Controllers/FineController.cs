using LibrarySystem.API.Dtos.FineDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FineController : ControllerBase
    {
        private readonly IFineService _fineService;
        private readonly ILogger<FineController> _logger;

        public FineController(IFineService fineService, ILogger<FineController> logger)
        {
            _fineService = fineService;
            _logger = logger;
        }

        [HttpPost("issue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IssueFine([FromBody] CreateFineDto fineDto)
        {
            try
            {
                var createdFine = await _fineService.AddFineAsync(fineDto);

                return Ok(createdFine);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bir sunucu hatası oluştu. {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserFinesByEmail([FromQuery] string email)
        {
            _logger.LogInformation("Admin tarafından kullanıcı cezaları sorgulanıyor. Email: {Email}", email);

            try
            {
                var fines = await _fineService.GetUserFinesByEmailAsync(email);
                return Ok(fines);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ceza sorgulama başarısız: Kullanıcı bulunamadı. Email: {Email}", email);
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("my-active-fines")]
        public async Task<IActionResult> GetActiveFines([FromQuery] FinePageableDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Aktif ceza sorgusu alındı. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
                userId, dto.page, dto.pageSize);

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Aktif ceza sorgusu başarısız: Token içerisinden UserId alınamadı.");
                return Unauthorized("Kullanıcı doğrulanamadı.");
            }

            try
            {
                var result = await _fineService.GetActiveFinesByUserIdAsync(userId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Aktif ceza sorgusu sırasında geçersiz istek hatası. UserId: {UserId}", userId);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Aktif ceza bulunamadı. UserId: {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aktif ceza sorgusu sırasında beklenmeyen bir hata oluştu. UserId: {UserId}", userId);
                return StatusCode(500, "Aktif cezalar alınırken bir hata oluştu.");
            }
        }


        [Authorize]
        [HttpGet("my-history-fines")]
        public async Task<IActionResult> GetInactiveFines([FromQuery] FinePageableDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Pasif ceza sorgusu alındı. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
                userId, dto.page, dto.pageSize);

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Pasif ceza sorgusu başarısız: Token içerisinden UserId alınamadı.");
                return Unauthorized("Kullanıcı doğrulanamadı.");
            }

            try
            {
                var result = await _fineService.GetInActiveFinesByUserIdAsync(userId, dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Ceza kayıtları sorgusu sırasında geçersiz istek hatası. UserId: {UserId}", userId);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Ceza kayıtları bulunamadı. UserId: {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ceza kayıtları sorgusu sırasında beklenmeyen bir hata oluştu. UserId: {UserId}", userId);
                return StatusCode(500, "Ceza kayıtları alınırken bir hata oluştu.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("revoke/{fineId}")]
        public async Task<IActionResult> RevokeFine(int fineId)
        {
            _logger.LogInformation("Ceza kaldırma işlemi başlatıldı. FineId: {FineId}", fineId);

            try
            {
                var fine = await _fineService.RevokeFineAsync(fineId);

                _logger.LogInformation("Ceza kaldırma işlemi başarılı. FineId: {FineId}", fineId);

                return Ok(fine);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ceza kaldırma başarısız: Ceza bulunamadı. FineId: {FineId}", fineId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ceza ödeme işlemi sırasında beklenmeyen bir hata oluştu. FineId: {FineId}", fineId);
                return StatusCode(500, "Ceza ödenirken bir hata oluştu.");
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost("pay")]
        public async Task<IActionResult> PayFine([FromQuery] int fineId)
        {
            _logger.LogInformation("Ceza kaldırma işlemi başlatıldı. FineId: {FineId}", fineId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Ceza ödeme işlemi başarısız: Token içerisinden UserId alınamadı.");
                return Unauthorized("Kullanıcı doğrulanamadı. Lütfen giriş yapınız.");
            }
            try
            {
                var fine = await _fineService.PayFineAsync(userId,fineId);

                _logger.LogInformation("Ceza kaldırma işlemi başarılı. FineId: {FineId}", fineId);

                return Ok(fine);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ceza kaldırma başarısız: Ceza bulunamadı. FineId: {FineId}", fineId);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Ceza ödeme başarısız: Ceza kullanıcısı ile ödeme yapan kullanıcı uyuşmuyor. FineId: {FineId}, UserId: {UserId}", fineId, userId);
                return StatusCode(403,ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ceza ödeme işlemi sırasında beklenmeyen bir hata oluştu. FineId: {FineId}, UserId: {UserId}", fineId, userId);
                return StatusCode(500, "Ceza ödenirken bir hata oluştu.");
            }
        }
    }
}