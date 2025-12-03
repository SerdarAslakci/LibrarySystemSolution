using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        }
    }
}