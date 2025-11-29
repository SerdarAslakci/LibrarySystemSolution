using LibrarySystem.API.ServiceInterfaces;
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
    }
}
