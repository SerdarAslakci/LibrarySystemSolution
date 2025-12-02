using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "Admin")]
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
                return BadRequest("Bu kategori zaten mevcut.");
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

        [HttpGet("by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("Kategori arama başarısız: 'name' parametresi boş gönderildi.");
                return BadRequest(new { message = "Aranacak kategori adı boş olamaz." });
            }

            try
            {
                _logger.LogInformation("Controller: Kategori ismiyle arama isteği alındı. Aranan: {SearchTerm}", name);

                var categories = await _categoryService.GetByNameAsync(name);

                if (!categories.Any())
                {
                    _logger.LogInformation("Aranan isimde kategori bulunamadı: {SearchTerm}", name);
                }
                
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori aranırken sunucu hatası oluştu. Aranan: {SearchTerm}", name);
                return StatusCode(500, "Sunucu hatası.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {

            _logger.LogInformation("Category ID bilgisine göre silme isteği alındı. ID: {Id}", id);

            try
            {
                var isDeleted = await _categoryService.DeleteCategoryByIdAsync(id);
                return Ok(isDeleted);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Silinmek istenen kategori bulunamadı. ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori silinirken sunucu hatası oluştu. ID: {Id}", id);
                return StatusCode(500, "Sunucu hatası.");
            }
        }

        [HttpGet("pageable")]
        public async Task<IActionResult> GetAllPageable([FromQuery] CategoryPageableDto pageableDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Geçersiz sayfalama parametreleri alındı.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation(
                    "Controller: Sayfalandırılmış kategori isteği alındı. Sayfa: {Page}, Sayfa Boyutu: {PageSize}",
                    pageableDto.page,
                    pageableDto.pageSize
                );

                var pageableCategoriesResult = await _categoryService.GetAllCategoriesPageableAsync(pageableDto.page, pageableDto.pageSize);

                _logger.LogInformation(
                    "Controller: Sayfalandırılmış kategori listeleme başarıyla tamamlandı. Toplam: {TotalCount}, Toplam Sayfa: {TotalPages}",
                    pageableCategoriesResult.TotalCount,
                    pageableCategoriesResult.TotalPages
                );

                return Ok(pageableCategoriesResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Controller: Sayfalandırılmış kategorileri getirirken beklenmedik bir hata oluştu.");

                return StatusCode(500, "Sunucu tarafında bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto categoryDto)
        {
            _logger.LogInformation("Kategori güncelleme isteği alındı. ID: {Id}", id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Kategori güncelleme işlemi başarısız. Geçersiz model durumu. ID: {Id}, Hatalar: {Errors}",
                    id, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                return BadRequest(ModelState);
            }

            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);

                _logger.LogInformation("Kategori başarıyla güncellendi. ID: {Id}", id);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Güncellenmek istenen kategori bulunamadı. ID: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori güncellenirken sunucu taraflı kritik bir hata oluştu. ID: {Id}", id);
                return StatusCode(500, new { message = "İşlem sırasında sunucu kaynaklı bir hata oluştu. Lütfen daha sonra tekrar deneyiniz." });
            }
        }
    }
}
