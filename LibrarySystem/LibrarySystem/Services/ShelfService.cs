using LibrarySystem.API.Dtos.ShelfDtos;
using LibrarySystem.API.Repositories;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class ShelfService : IShelfService
    {
        private readonly IShelfRepository _shelfRepository;
        private readonly IRoomService _roomService;
        public ShelfService(IShelfRepository shelfRepository, IRoomService roomService)
        {
            _shelfRepository = shelfRepository;
            _roomService = roomService;
        }

        public async Task<Shelf> AddShelfAsync(CreateShelfDto shelfDto)
        {
            var roomExists = await _roomService.GetRoomByIdAsync(shelfDto.RoomId);
            if (roomExists == null)
            {
                throw new KeyNotFoundException($"ID {shelfDto.RoomId} ile kayıtlı Oda/Salon bulunamadı.");
            }

            var existingShelf = await _shelfRepository.GetShelfByCodeAndRoomIdAsync(
                shelfDto.ShelfCode, shelfDto.RoomId);

            if (existingShelf != null)
            {
                throw new InvalidOperationException($"'{shelfDto.ShelfCode}' kodlu raf, ID {shelfDto.RoomId} olan odada zaten mevcuttur.");
            }

            var shelf = new Shelf
            {
                ShelfCode = shelfDto.ShelfCode,
                RoomId = shelfDto.RoomId
            };

            return await _shelfRepository.AddShelfAsync(shelf);
        }

        public async Task<Shelf?> GetShelfByCodeAndRoomIdAsync(string shelfCode, int roomId)
        {
            if (string.IsNullOrWhiteSpace(shelfCode))
                throw new ArgumentException("Shelf code cannot be null or empty.", nameof(shelfCode));

            var shelf = await _shelfRepository.GetShelfByCodeAndRoomIdAsync(shelfCode, roomId);

            return shelf;
        }
    }
}
