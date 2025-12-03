using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilterDto filter)
        {
            _logger.LogInformation("Kullanıcı listesi sorgulanıyor. Sayfa: {Page}, Filtreler: {@Filter}", filter.Page, filter);

            try
            {
                var result = await _userService.GetUsersForListingAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı listeleme sırasında hata.");
                return StatusCode(500,"Sunucu hatası oluştu.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserDetailByIdAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("{UserId} id'li kullanıcı bulunamadı.", id);
                    return NotFound("Kullanıcı bulunamadı.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı (ID) sorgulama hatası: {UserId}", id);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            _logger.LogInformation("Email ile kullanıcı detayı sorgulanıyor: {Email}", email);
            try
            {
                var user = await _userService.GetUserDetailByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("{Email} adresli kullanıcı bulunamadı.", email);
                    return NotFound("Kullanıcı bulunamadı.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı (Email) sorgulama hatası: {Email}", email);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                _logger.LogWarning("Mevcut kullanıcı kimliği alınamadı.");
                return Unauthorized("Kullanıcı kimliği alınamadı.");
            }

            try
            {
                _logger.LogInformation("Mevcut kullanıcı detayı sorgulanıyor: {UserId}", userId);
                var user = await _userService.GetUserDetailByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Mevcut kullanıcı bulunamadı: ID '{UserId}'", userId);
                    return NotFound("Kullanıcı bulunamadı.");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mevcut kullanıcı sorgulama hatası: {UserId}", userId);
                return StatusCode(500, ex.Message);
            }
        }
        [Authorize]
        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            _logger.LogInformation("Kullanıcı istatistikleri isteği alındı.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Kimlik doğrulama başarısız: Yetkilendirme talebinde User ID ClaimType eksik veya boş.");
                return Unauthorized("Kimlik doğrulama bilgisi eksik veya geçersiz.");
            }

            _logger.LogInformation("Kullanıcı istatistikleri sorgulanıyor. User ID: {UserId}", userId);

            try
            {
                var userStats = await _userService.GetUserStatsAsync(userId);

                _logger.LogInformation("Kullanıcı istatistikleri başarıyla alındı. User ID: {UserId}", userId);
                return Ok(userStats);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Kullanıcı istatistikleri sorgulama başarısız. Kullanıcı bulunamadı. User ID: {UserId}", userId);

                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı istatistikleri alınırken beklenmeyen bir sunucu hatası oluştu. User ID: {UserId}", userId);

                return StatusCode(StatusCodes.Status500InternalServerError,
                                  $"İstatistikler alınırken beklenmeyen bir hata oluştu. Hata: {ex.Message}");
            }
        }
    }
}