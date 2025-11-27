using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryCreateDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Kategori ekleme isteği geçersiz model durumuyla geldi.");
                return BadRequest(ModelState);
            }

            var category = new Category { Name = categoryDto.Name };

            try
            {
                var addedCategory = await _categoryService.AddCategoryAsync(category);

                _logger.LogInformation("Kategori ekleme başarılı. Kategori ID: {Id}", addedCategory.Id);

                var resultDto = new CategoryResultDto { Id = addedCategory.Id, Name = addedCategory.Name };

                return CreatedAtAction(nameof(GetCategoryById), new { id = addedCategory.Id }, resultDto);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Kategori eklenirken null argüman hatası.");
                return BadRequest("Kategori verisi boş olamaz.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Kategori ekleme başarısız: Zaten mevcut. İsim: {Name}", category.Name);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori eklenirken beklenmeyen bir sunucu hatası oluştu.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Kategori eklenemedi.");
            }
        }

        [HttpGet]
        [Route("list")]
        public async Task<IActionResult> GetAllCategories()
        {
            _logger.LogInformation("GET /api/category isteği alındı. Tüm kategoriler isteniyor.");

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori listesi getirilirken beklenmeyen bir sunucu hatası oluştu.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Kategori listesi getirilemedi.");
            }
        }

        [HttpGet("{id:int}", Name = "GetCategoryById")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            _logger.LogInformation("GET /api/category/{Id} isteği alındı.", id);

            try
            {
                var category = await _categoryService.GetByIdAsync(id);

                var resultDto = new CategoryResultDto { Id = category.Id, Name = category.Name };
                return Ok(resultDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "ID: {Id} ile kategori bulunamadı.", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID: {Id} ile kategori getirilirken hata oluştu.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Kategori getirilemedi.");
            }
        }
    }
}
