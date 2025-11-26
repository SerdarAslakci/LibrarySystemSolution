using LibrarySystem.API.Dtos.ShelfDtos;
using LibrarySystem.API.Repositories;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class ShelfService : IShelfService
    {
        private readonly IShelfRepository _shelfRepository;
        private readonly IRoomService _roomService;
        private readonly ILogger<ShelfService> _logger;

        public ShelfService(IShelfRepository shelfRepository, IRoomService roomService, ILogger<ShelfService> logger)
        {
            _shelfRepository = shelfRepository;
            _roomService = roomService;
            _logger = logger;
        }

        public async Task<Shelf> AddShelfAsync(CreateShelfDto shelfDto)
        {
            _logger.LogInformation("Raf ekleme işlemi başlatıldı. Kod: {ShelfCode}, OdaId: {RoomId}", shelfDto?.ShelfCode, shelfDto?.RoomId);

            var roomExists = await _roomService.GetRoomByIdAsync(shelfDto.RoomId);
            if (roomExists == null)
            {
                _logger.LogWarning("Raf ekleme başarısız: Oda bulunamadı. RoomId: {RoomId}", shelfDto.RoomId);
                throw new KeyNotFoundException($"ID {shelfDto.RoomId} ile kayıtlı Oda/Salon bulunamadı.");
            }

            var existingShelf = await _shelfRepository.GetShelfByCodeAndRoomIdAsync(
                shelfDto.ShelfCode, shelfDto.RoomId);

            if (existingShelf != null)
            {
                _logger.LogWarning("Raf ekleme başarısız: Raf zaten mevcut. Kod: {ShelfCode}, OdaId: {RoomId}", shelfDto.ShelfCode, shelfDto.RoomId);
                throw new InvalidOperationException($"'{shelfDto.ShelfCode}' kodlu raf, ID {shelfDto.RoomId} olan odada zaten mevcuttur.");
            }

            var shelf = new Shelf
            {
                ShelfCode = shelfDto.ShelfCode,
                RoomId = shelfDto.RoomId
            };

            var result = await _shelfRepository.AddShelfAsync(shelf);

            _logger.LogInformation("Raf başarıyla eklendi. ID: {Id}", result.Id);

            return result;
        }

        public async Task<Shelf?> GetShelfByCodeAndRoomIdAsync(string shelfCode, int roomId)
        {
            if (string.IsNullOrWhiteSpace(shelfCode))
            {
                _logger.LogWarning("Raf sorgulama hatası: Raf kodu boş girildi.");
                throw new ArgumentException("Shelf code cannot be null or empty.", nameof(shelfCode));
            }

            var shelf = await _shelfRepository.GetShelfByCodeAndRoomIdAsync(shelfCode, roomId);

            return shelf;
        }
    }
}