using LibrarySystem.API.Dtos.BookCopyDtos;
using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [Authorize]
        [HttpPost]
        [Route("add-book")]
        public async Task<IActionResult> AddBook([FromBody] CreateBookDto dto)
        {
            if (dto == null)
                return BadRequest("Kitap bilgisi boş olamaz.");

            try
            {
                var book = await _bookService.AddBookAsync(dto);
                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Beklenmedik bir hata oluştu: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut]
        [Route("update-book/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] CreateBookDto dto)
        {
            if(!ModelState.IsValid)
                return BadRequest(new { Success = false, Message = "Geçersiz model durumu.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var updatedBook = await _bookService.UpdateBookAsync(id, dto);
                return Ok(updatedBook);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
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
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap ID'si." });

            try
            {
                var result = await _bookService.DeleteBookAsync(id);
                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Kitap başarıyla silindi."
                    });
                }
                return NotFound(new { Success = false, Message = "Kitap bulunamadı." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
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
                    return NotFound(new { Success = false, Message = "Kitap bulunamadı." });

                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Beklenmedik bir hata oluştu.",
                    Detail = ex.Message
                });
            }
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
                    return NotFound(new { Success = false, Message = "Kitap bulunamadı." });

                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
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
            try
            {
                var books = await _bookService.GetAllBooksAsync(filterDto);
                return Ok(books);
            }
            catch (Exception ex)
            {
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
            if (bookAuthor == null)
                return BadRequest(new { Success = false, Message = "Kitap-Yazar bilgisi boş olamaz." });

            try
            {
                var added = await _bookService.AddBookAuthorAsync(bookAuthor);
                return Ok(added);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
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
            if (dto == null)
                return BadRequest(new { Success = false, Message = "Kitap kopyası bilgisi boş olamaz." });

            try
            {
                var bookCopy = await _bookService.AddBookCopyAsync(dto);
                return Ok(bookCopy);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
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
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap kopyası ID'si." });

            if (dto == null)
                return BadRequest(new { Success = false, Message = "Güncelleme bilgisi boş olamaz." });

            try
            {
                var updatedCopy = await _bookService.UpdateBookCopyAsync(id, dto);
                return Ok(updatedCopy);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
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
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Geçersiz kitap kopyası ID'si." });

            try
            {
                var result = await _bookService.DeleteBookCopyAsync(id);
                if (result)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Kitap kopyası başarıyla silindi."
                    });
                }
                return NotFound(new { Success = false, Message = "Kitap kopyası bulunamadı." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
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
