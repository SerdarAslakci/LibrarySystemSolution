using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
                return StatusCode(500, new { message = "Sunucu hatası oluştu." });
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
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı (ID) sorgulama hatası: {UserId}", id);
                return StatusCode(500, new { message = ex.Message });
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
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı (Email) sorgulama hatası: {Email}", email);
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}