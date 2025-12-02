using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookService bookService, ILogger<BookController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        [Route("add-book")]
        public async Task<IActionResult> AddBook([FromBody] CreateBookDto dto)
        {
            _logger.LogInformation("Kitap ekleme isteği alındı. Başlık: {Title}, ISBN: {ISBN}", dto?.Title, dto?.ISBN);

            if (dto == null)
            {
                _logger.LogWarning("Kitap ekleme başarısız: Gönderilen veri boş.");
                return BadRequest("Kitap bilgisi boş olamaz.");
            }

            try
            {
                var book = await _bookService.AddBookAsync(dto);
                _logger.LogInformation("Kitap başarıyla eklendi. ID: {BookId}", book.Id);
                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Kitap ekleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Kitap ekleme hatası (Çakışma): {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Kitap ekleme hatası (Bulunamadı): {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap ekleme sırasında sunucu hatası.");
                return StatusCode(500, $"Beklenmedik bir hata oluştu: {ex.Message}");
            }
        }


        [HttpGet("get-by-name")]
        public async Task<IActionResult> GetBookByNameWithDetails([FromQuery] string name)
        {
            _logger.LogInformation("Controller: Kitap ismiyle detaylı arama isteği alındı. Parametre: {Name}", name);

            try
            {
                var result = await _bookService.GetBooksByNameWithDetailsAsync(name);

                if (result == null)
                {
                    _logger.LogWarning("Controller: Aranan isimle eşleşen kitap bulunamadı. Parametre: {Name}", name);
                    return NotFound(new { message = $"'{name}' ismine benzer veya eşleşen bir kitap bulunamadı." });
                }

                _logger.LogInformation("Controller: Kitap başarıyla bulundu ve dönülüyor");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Controller: Geçersiz arama parametresi.");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Kitap aranırken beklenmeyen bir hata oluştu.");
                return StatusCode(500, new { message = "Sunucu hatası oluştu." });
            }
        }

        [HttpGet("other-by-author")]
        public async Task<IActionResult> GetOtherBooksByAuthor([FromQuery] AuthorBooksRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var books = await _bookService.GetOtherBooksByAuthorAsync(request.AuthorId, request.Size, request.CategoryId);
                return Ok(books);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Beklenmeyen bir hata oluştu.", detail = ex.Message });
            }
        }

        [Authorize]
        [HttpPut]
        [Route("update-book/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] CreateBookDto dto)
        {
            _logger.LogInformation("Kitap güncelleme isteği. ID: {BookId}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Kitap güncelleme validasyon hatası. ID: {BookId}", id);
                return BadRequest(new { Success = false, Message = "Geçersiz model durumu.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var updatedBook = await _bookService.UpdateBookAsync(id, dto);
                _logger.LogInformation("Kitap başarıyla güncellendi. ID: {BookId}", id);
                return Ok(updatedBook);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Kitap güncelleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kitap güncelleme hatası (Bulunamadı): ID {BookId}", id);
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap güncelleme sırasında sunucu hatası. ID: {BookId}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("delete-book/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            _logger.LogInformation("Kitap silme isteği. ID: {BookId}", id);

            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap ID'si." });

            try
            {
                var result = await _bookService.DeleteBookAsync(id);
                if (result)
                {
                    _logger.LogInformation("Kitap başarıyla silindi. ID: {BookId}", id);
                    return Ok(new
                    {
                        Success = true,
                        Message = "Kitap başarıyla silindi."
                    });
                }
                _logger.LogWarning("Kitap silinemedi (Bulunamadı veya işlem başarısız). ID: {BookId}", id);
                return NotFound(new { Success = false, Message = "Kitap bulunamadı." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kitap silme hatası (Bulunamadı): {Message}", ex.Message);
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap silme sırasında sunucu hatası. ID: {BookId}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("get-book/{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap ID'si." });

            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    _logger.LogWarning("Kitap getirme başarısız: ID {BookId} bulunamadı.", id);
                    return NotFound(new { Success = false, Message = "Kitap bulunamadı." });
                }

                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap getirme sırasında sunucu hatası. ID: {BookId}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet("all-book-copies")]
        public async Task<IActionResult> GetBookCopies([FromQuery] BookCopyFilterDto filter)
        {
            _logger.LogInformation(
                "Kitap kopyaları için istek alındı. BookId: {BookId}, Page: {Page}, PageSize: {PageSize}",
                filter.BookId, filter.page, filter.pageSize
            );

            if (filter.page <= 0 || filter.pageSize <= 0)
            {
                _logger.LogWarning(
                    "Geçersiz pagination parametreleri. Page: {Page}, PageSize: {PageSize}",
                    filter.page, filter.pageSize
                );
                return BadRequest("Page ve PageSize sıfırdan büyük olmalıdır.");
            }

            var result = await _bookService.GetAllBookCopiesByIdAsync(filter);

            return Ok(result);
        }


        [HttpGet]
        [Route("get-book-details/{id}")]
        public async Task<IActionResult> GetBookWithDetails(int id)
        {
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap ID'si." });

            try
            {
                var book = await _bookService.GetBookWithDetailsAsync(id);
                if (book == null)
                {
                    _logger.LogWarning("Detaylı kitap getirme başarısız: ID {BookId} bulunamadı.", id);
                    return NotFound(new { Success = false, Message = "Kitap bulunamadı." });
                }

                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detaylı kitap getirme sırasında sunucu hatası. ID: {BookId}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("get-all-books")]
        public async Task<IActionResult> GetAllBooks([FromQuery] BookFilterDto filterDto)
        {
            _logger.LogInformation("Tüm kitapları getirme isteği. Filtreler: Başlık={Title}, Kategori={Category}, Page={Page}", filterDto?.Title, filterDto?.CategoryId, filterDto?.Page);

            try
            {
                var books = await _bookService.GetAllBooksAsync(filterDto);
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm kitapları getirme sırasında sunucu hatası.");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("add-book-author")]
        public async Task<IActionResult> AddBookAuthor([FromBody] BookAuthor bookAuthor)
        {
            _logger.LogInformation("Kitap-Yazar ilişkisi ekleme isteği. BookId: {BookId}, AuthorId: {AuthorId}", bookAuthor?.BookId, bookAuthor?.AuthorId);

            if (bookAuthor == null)
                return BadRequest(new { Success = false, Message = "Kitap-Yazar bilgisi boş olamaz." });

            try
            {
                var added = await _bookService.AddBookAuthorAsync(bookAuthor);
                _logger.LogInformation("Kitap-Yazar ilişkisi eklendi.");
                return Ok(added);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Kitap-Yazar ekleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Kitap-Yazar ekleme hatası (Çakışma): {Message}", ex.Message);
                return Conflict(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap-Yazar ekleme sırasında sunucu hatası.");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("is-book-author-exists/{bookId}/{authorId}")]
        public async Task<IActionResult> IsBookAuthorExists(int bookId, int authorId)
        {
            if (bookId <= 0 || authorId <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap veya yazar ID'si." });

            try
            {
                var exists = await _bookService.IsBookAuthorExistsAsync(bookId, authorId);
                return Ok(exists);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap-Yazar kontrolü sırasında sunucu hatası.");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost]
        [Route("add-book-copy")]
        public async Task<IActionResult> AddBookCopy([FromBody] CreateBookCopyDto dto)
        {
            _logger.LogInformation("Kitap kopyası ekleme isteği. BookId: {BookId}, Barkod: {Barcode}", dto?.BookId, dto?.BarcodeNumber);

            if (dto == null)
                return BadRequest(new { Success = false, Message = "Kitap kopyası bilgisi boş olamaz." });

            try
            {
                var bookCopy = await _bookService.AddBookCopyAsync(dto);
                _logger.LogInformation("Kitap kopyası eklendi. ID: {CopyId}", bookCopy.Id);
                return Ok(bookCopy);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kitap kopyası ekleme hatası (Bulunamadı): {Message}", ex.Message);
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap kopyası ekleme sırasında sunucu hatası.");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPut]
        [Route("update-book-copy/{id}")]
        public async Task<IActionResult> UpdateBookCopy(int id, [FromBody] UpdateBookCopyDto dto)
        {
            _logger.LogInformation("Kitap kopyası güncelleme isteği. CopyId: {CopyId}", id);

            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap kopyası ID'si." });

            if (dto == null)
                return BadRequest(new { Success = false, Message = "Güncelleme bilgisi boş olamaz." });

            try
            {
                var updatedCopy = await _bookService.UpdateBookCopyAsync(id, dto);
                _logger.LogInformation("Kitap kopyası güncellendi. CopyId: {CopyId}", id);
                return Ok(updatedCopy);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Kopya güncelleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kopya güncelleme hatası (Bulunamadı): ID {CopyId}", id);
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap kopyası güncelleme sırasında sunucu hatası. ID: {CopyId}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("delete-book-copy/{id}")]
        public async Task<IActionResult> DeleteBookCopy(int id)
        {
            _logger.LogInformation("Kitap kopyası silme isteği. CopyId: {CopyId}", id);

            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap kopyası ID'si." });

            try
            {
                var result = await _bookService.DeleteBookCopyAsync(id);
                if (result)
                {
                    _logger.LogInformation("Kitap kopyası silindi. CopyId: {CopyId}", id);
                    return Ok(new
                    {
                        Success = true,
                        Message = "Kitap kopyası başarıyla silindi."
                    });
                }
                _logger.LogWarning("Kitap kopyası silinemedi (Bulunamadı veya başarısız). ID: {CopyId}", id);
                return NotFound(new { Success = false, Message = "Kitap kopyası bulunamadı." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Kitap kopyası silme hatası (Bulunamadı): {Message}", ex.Message);
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kitap kopyası silme sırasında sunucu hatası. ID: {CopyId}", id);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
        }
    }
}