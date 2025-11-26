using LibrarySystem.API.Dtos.FineTypeDtos;
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
    public class FineTypeController : ControllerBase
    {
        private readonly IFineTypeService _fineTypeService;
        private readonly ILogger<FineTypeController> _logger;

        public FineTypeController(IFineTypeService fineTypeService, ILogger<FineTypeController> logger)
        {
            _fineTypeService = fineTypeService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllFineTypes()
        {
            _logger.LogInformation("Tüm ceza tipleri listeleniyor.");
            var fineTypes = await _fineTypeService.GetAllFineTypesAsync();
            return Ok(fineTypes);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var fineType = await _fineTypeService.GetByIdAsync(id);
                return Ok(fineType);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ceza tipi sorgulama başarısız: ID {Id} bulunamadı.", id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Ceza tipi sorgulama hatası (Argüman): {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddFineType([FromBody] CreateFineTypeDto fineTypeDto)
        {
            _logger.LogInformation("Yeni ceza tipi ekleme isteği: {Name}", fineTypeDto?.Name);

            if (fineTypeDto == null)
                return BadRequest(new { message = "Ceza tipi boş olamaz." });

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Ceza tipi ekleme validasyon hatası.");
                return BadRequest(ModelState);
            }

            try
            {
                var added = await _fineTypeService.AddFineTypeAsync(fineTypeDto);
                _logger.LogInformation("Ceza tipi başarıyla eklendi. ID: {Id}", added.Id);
                return CreatedAtAction(nameof(GetById), new { id = added.Id }, added);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Ceza tipi ekleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ceza tipi eklenirken sunucu hatası.");
                return StatusCode(500, new { message = "Ceza tipi eklenirken bir hata oluştu." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateFineType([FromBody] UpdateFineTypeDto fineType)
        {
            _logger.LogInformation("Ceza tipi güncelleme isteği. ID: {Id}", fineType?.Id);

            if (fineType == null)
                return BadRequest(new { message = "Geçersiz ceza tipi" });

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Ceza tipi güncelleme validasyon hatası. ID: {Id}", fineType.Id);
                return BadRequest(ModelState);
            }

            try
            {
                var updated = await _fineTypeService.UpdateFineTypeAsync(fineType);
                _logger.LogInformation("Ceza tipi güncellendi. ID: {Id}", updated.Id);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ceza tipi güncelleme hatası (Bulunamadı): ID {Id}", fineType.Id);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Ceza tipi güncelleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ceza tipi güncellenirken sunucu hatası. ID: {Id}", fineType.Id);
                return StatusCode(500, new { message = "Ceza tipi güncellenirken bir hata oluştu." });
            }
        }
    }
}