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

        public async Task<Shelf> GetShelfByIdAsync(int shelfId)
        {
            var shelf = await _shelfRepository.GetShelfByIdAsync(shelfId);

            if (shelf == null)
            {
                _logger.LogWarning("Raf arama başarısız: ID {ShelfId} bulunamadı.", shelfId);
                throw new KeyNotFoundException($"ID değeri {shelfId} olan raf bulunamadı.");
            }

            return shelf;
        }

        public async Task<IEnumerable<Shelf>> GetShelvesByRoomIdAsync(int roomId)
        {
            _logger.LogInformation("Oda ID: {RoomId} için raflar listeleniyor.", roomId);

            var shelves = await _shelfRepository.GetShelvesByRoomIdAsync(roomId);

            if (!shelves.Any())
            {
                _logger.LogInformation("Oda ID: {RoomId} için kayıtlı raf bulunamadı.", roomId);
            }

            return shelves;
        }

        public async Task<Shelf> UpdateShelfAsync(int id, UpdateShelfDto shelfDto)
        {
            _logger.LogInformation("Raf güncelleme isteği: ID {ShelfId}, Yeni Kod: {Code}, Yeni Oda: {RoomId}", id, shelfDto.ShelfCode, shelfDto.RoomId);

            var existingShelf = await _shelfRepository.GetShelfByIdAsync(id);
            if (existingShelf == null)
            {
                _logger.LogWarning("Raf güncelleme başarısız: ID {ShelfId} bulunamadı.", id);
                throw new KeyNotFoundException($"ID değeri {id} olan raf bulunamadı.");
            }

            if (existingShelf.ShelfCode != shelfDto.ShelfCode || existingShelf.RoomId != shelfDto.RoomId)
            {
                var duplicateCheck = await _shelfRepository.GetShelfByCodeAndRoomIdAsync(shelfDto.ShelfCode, shelfDto.RoomId);

                if (duplicateCheck != null && duplicateCheck.Id != id)
                {
                    _logger.LogWarning("Raf güncelleme hatası: {RoomId} nolu odada {Code} kodlu raf zaten var.", shelfDto.RoomId, shelfDto.ShelfCode);
                    throw new InvalidOperationException($"'{shelfDto.RoomId}' ID'li odada '{shelfDto.ShelfCode}' koduna sahip başka bir raf zaten mevcut.");
                }
            }

            existingShelf.ShelfCode = shelfDto.ShelfCode;
            existingShelf.RoomId = shelfDto.RoomId;

            var updatedShelf = await _shelfRepository.UpdateShelfAsync(existingShelf);

            _logger.LogInformation("Raf başarıyla güncellendi: ID {ShelfId}", id);
            return updatedShelf;
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