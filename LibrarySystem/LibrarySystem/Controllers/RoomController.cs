using LibrarySystem.API.Dtos.RoomDtos;
using LibrarySystem.API.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomController> _logger;

        public RoomController(IRoomService roomService, ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Tüm odalar listeleniyor.");
            var rooms = await _roomService.GetAllAsync();
            return Ok(rooms);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var room = await _roomService.GetRoomByIdAsync(id);
                return Ok(room);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Oda sorgulama hatası (Argüman): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Oda sorgulama başarısız: ID {Id} bulunamadı.", id);
                return NotFound(ex.Message);
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateRoomDto roomDto)
        {
            _logger.LogInformation("Yeni oda ekleme isteği. Kod: {RoomCode}", roomDto?.RoomCode);

            try
            {
                var addedRoom = await _roomService.AddAsync(roomDto);
                _logger.LogInformation("Oda başarıyla eklendi. ID: {Id}", addedRoom.Id);
                return Ok(addedRoom);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Oda ekleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Oda ekleme hatası (Çakışma): {Message}", ex.Message);
                return Conflict(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateRoomDto roomDto)
        {
            _logger.LogInformation("Oda güncelleme isteği. ID: {Id}", id);

            try
            {
                var updatedRoom = await _roomService.UpdateAsync(id, roomDto);
                _logger.LogInformation("Oda başarıyla güncellendi. ID: {Id}", id);
                return Ok(updatedRoom);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Oda güncelleme hatası (Argüman): {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Oda güncelleme hatası (Çakışma): {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Oda güncelleme hatası (Bulunamadı): ID {Id}", id);
                return NotFound(ex.Message);
            }
        }
    }
}