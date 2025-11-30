using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(IAuthorService authorService, ILogger<AuthorsController> logger)
        {
            _authorService = authorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            { 
                var authors = await _authorService.GetAllAuthorsAsync();

                return Ok(authors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Yazarları getirirken kritik bir hata oluştu.");

                return StatusCode(500, "Sunucu tarafında bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var author = await _authorService.GetByIdAsync(id);
                return Ok(author);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Yazar bulunamadı. ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yazar getirilirken beklenmedik bir hata oluştu.");
                return StatusCode(500, "Sunucu hatası.");
            }
        }


        [HttpGet("by-name")]
        public async Task<IActionResult> GetAuthorsByName([FromQuery] string? firstName, [FromQuery] string? lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                _logger.LogWarning("Yazar arama başarısız: Hem Ad hem Soyad boş geçilmiş.");
                return BadRequest(new { message = "Arama yapmak için en az bir isim veya soyisim girmelisiniz." });
            }

            try
            {
                _logger.LogInformation("Controller: Yazar arama isteği alındı. Ad: {FirstName}, Soyad: {LastName}", firstName, lastName);
                var authors = await _authorService.GetAuthorsByNameAsync(firstName, lastName);

                return Ok(authors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yazar aranırken beklenmedik bir sunucu hatası oluştu. Ad: {FirstName}, Soyad: {LastName}", firstName, lastName);
                return StatusCode(500, "Sunucu hatası.");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto authorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdAuthor = await _authorService.AddAuthorAsync(authorDto);

                return Ok(createdAuthor);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Eksik argüman hatası.");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Geçersiz argüman hatası.");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Çakışma hatası (Duplicate).");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yazar eklenirken beklenmedik bir hata oluştu.");
                return StatusCode(500, "Sunucu hatası oluştu.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {

            _logger.LogInformation("Author ID bilgisine göre silme isteği alındı. ID: {Id}", id);
            try
            {
                var isDeleted = await _authorService.DeleteAuthorByIdAsync(id);

                return Ok(isDeleted);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Silme işlemi sırasında yazar bulunamadı. ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yazar silinirken beklenmedik bir hata oluştu. ID: {Id}", id);
                return StatusCode(500, "Sunucu hatası.");
            }
        }
    }
}
