using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> GetUsers([FromQuery] string? role = null)
        {
            _logger.LogInformation("Kullanıcı listesi sorgulanıyor. Filtre: {Role}", role ?? "Tümü");
            try
            {
                var users = await _userService.GetUsersForListingAsync(role);
                return Ok(users);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kullanıcı listeleme uyarısı: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserDetailByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kullanıcı sorgulama (ID) başarısız: {UserId} bulunamadı.", id);
                return NotFound(new { message = ex.Message });
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
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kullanıcı sorgulama (Email) başarısız: {Email} bulunamadı.", email);
                return NotFound(new { message = ex.Message });
            }
        }
    }
}