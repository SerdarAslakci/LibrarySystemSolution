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

        [HttpGet("pageable")]
        public async Task<IActionResult> GetAllPageable([FromQuery] AuthorPageableDto pageableDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Geçersiz sayfalama parametreleri alındı.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation(
                    "Controller: Sayfalandırılmış yazar isteği alındı. Sayfa: {Page}, Sayfa Boyutu: {PageSize}",
                    pageableDto.page,
                    pageableDto.pageSize
                );

                var pageableAuthorsResult = await _authorService.GetAllAuthorsPageableAsync(pageableDto.page,pageableDto.pageSize);

                _logger.LogInformation(
                    "Controller: Sayfalandırılmış yazar listeleme başarıyla tamamlandı. Toplam: {TotalCount}",
                    pageableAuthorsResult.TotalCount
                );

                return Ok(pageableAuthorsResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Sayfalandırılmış yazarları getirirken beklenmedik bir hata oluştu.");

                return StatusCode(500, "Sunucu tarafında bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] UpdateAuthorDto authorDto)
        {
            _logger.LogInformation("Yazar güncelleme isteği alındı. ID: {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Yazar güncelleme işlemi başarısız. Geçersiz model durumu. ID: {Id}, Hatalar: {Errors}",
                    id, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authorService.UpdateAuthorAsync(id, authorDto);

                _logger.LogInformation("Yazar güncelleme isteği başarıyla tamamlandı. ID: {Id}", id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Güncellenmek istenen yazar bulunamadı. ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yazar güncellenirken sunucu taraflı bir hata oluştu. ID: {Id}", id);
                return StatusCode(500, new { message = "İşlem sırasında sunucu kaynaklı bir hata oluştu. Lütfen daha sonra tekrar deneyiniz." });
            }
        }
    }
}
