using LibrarySystem.API.Dtos.ShelfDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShelfController : ControllerBase
    {
        private readonly IShelfService _shelfService;
        private readonly ILogger<ShelfController> _logger;

        public ShelfController(IShelfService shelfService, ILogger<ShelfController> logger)
        {
            _shelfService = shelfService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetShelfById(int id)
        {
            _logger.LogInformation("GET: Raf detayı isteniyor. ID: {ShelfId}", id);
            try
            {
                var shelf = await _shelfService.GetShelfByIdAsync(id);
                return Ok(shelf);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("GET: Raf bulunamadı. ID: {ShelfId}. Hata: {Message}", id, ex.Message);
                return NotFound( ex.Message );
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetShelvesByRoomId(int roomId)
        {
            _logger.LogInformation("GET: Oda bazlı raf listesi isteniyor. Oda ID: {RoomId}", roomId);

            var shelves = await _shelfService.GetShelvesByRoomIdAsync(roomId);

            _logger.LogInformation("GET: {Count} adet raf listelendi. Oda ID: {RoomId}", shelves.Count(), roomId);
            return Ok(shelves);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public async Task<IActionResult> GetShelfByCodeAndRoomId([FromQuery] string shelfCode, [FromQuery] int roomId)
        {
            _logger.LogInformation("GET: Raf arama isteği. Kod: {Code}, Oda ID: {RoomId}", shelfCode, roomId);
            try
            {
                var shelf = await _shelfService.GetShelfByCodeAndRoomIdAsync(shelfCode, roomId);

                if (shelf == null)
                {
                    _logger.LogWarning("GET: Kriterlere uygun raf bulunamadı. Kod: {Code}, Oda ID: {RoomId}", shelfCode, roomId);
                    return NotFound("Belirtilen kriterlere uygun raf bulunamadı.");
                }

                return Ok(shelf);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("GET: Geçersiz arama parametreleri. Hata: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateShelf([FromBody] CreateShelfDto createShelfDto)
        {
            _logger.LogInformation("POST: Yeni raf oluşturma isteği. Kod: {Code}, Oda ID: {RoomId}", createShelfDto.ShelfCode, createShelfDto.RoomId);
            try
            {
                var createdShelf = await _shelfService.AddShelfAsync(createShelfDto);

                _logger.LogInformation("POST: Raf başarıyla oluşturuldu. Yeni ID: {ShelfId}", createdShelf.Id);
                return CreatedAtAction(nameof(GetShelfById), new { id = createdShelf.Id }, createdShelf);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("POST: Raf oluşturulamadı (Bağlı kayıt yok). Hata: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("POST: Raf oluşturulamadı (Çakışma). Hata: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShelf(int id, [FromBody] UpdateShelfDto updateShelfDto)
        {
            _logger.LogInformation("PUT: Raf güncelleme isteği. ID: {ShelfId}", id);
            try
            {
                var updatedShelf = await _shelfService.UpdateShelfAsync(id, updateShelfDto);

                _logger.LogInformation("PUT: Raf başarıyla güncellendi. ID: {ShelfId}", id);
                return Ok(updatedShelf);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("PUT: Güncellenecek raf bulunamadı. ID: {ShelfId}", id);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("PUT: Raf güncelleme çakışması. ID: {ShelfId}. Hata: {Message}", id, ex.Message);
                return Conflict(ex.Message);
            }
        }
    }
}