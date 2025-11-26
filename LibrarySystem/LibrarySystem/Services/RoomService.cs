using LibrarySystem.API.Dtos.RoomDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class RoomService : IRoomService
    {

        private readonly IRoomRepository _roomRepository;
        private readonly ILogger<RoomService> _logger;

        public RoomService(IRoomRepository roomRepository, ILogger<RoomService> logger)
        {
            _roomRepository = roomRepository;
            _logger = logger;
        }

        public async Task<Room> AddAsync(CreateRoomDto room)
        {
            _logger.LogInformation("Yeni oda ekleme isteği: {RoomCode}", room?.RoomCode);

            if (room == null)
            {
                _logger.LogWarning("Oda ekleme başarısız: DTO boş.");
                throw new ArgumentNullException(nameof(room), "Oda bilgisi boş olamaz.");
            }

            if (string.IsNullOrWhiteSpace(room.RoomCode))
            {
                _logger.LogWarning("Oda ekleme başarısız: RoomCode boş.");
                throw new ArgumentException("RoomCode boş olamaz.");
            }

            if (string.IsNullOrWhiteSpace(room.Description))
            {
                _logger.LogWarning("Oda ekleme başarısız: Description boş.");
                throw new ArgumentException("Description boş olamaz.");
            }

            bool exists = await _roomRepository.ExistsAsync(room.RoomCode, room.Description);
            if (exists)
            {
                _logger.LogWarning("Oda ekleme başarısız: '{RoomCode}' ve açıklama kombinasyonu zaten mevcut.", room.RoomCode);
                throw new InvalidOperationException("Bu salon numarası ve açıklama kombinasyonuna sahip bir oda zaten mevcut.");
            }

            var roomToAdd = new Room
            {
                RoomCode = room.RoomCode,
                Description = room.Description
            };

            var addedRoom = await _roomRepository.AddAsync(roomToAdd);

            _logger.LogInformation("Oda başarıyla eklendi. ID: {Id}", addedRoom.Id);

            return addedRoom!;
        }


        public async Task<List<Room>> GetAllAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Geçersiz oda ID değeri.", nameof(id));

            var room = await _roomRepository.GetRoomByIdAsync(id);

            if (room == null)
            {
                _logger.LogWarning("Oda sorgulama başarısız: ID {Id} bulunamadı.", id);
                throw new KeyNotFoundException($"ID'si {id} olan oda bulunamadı.");
            }

            return room;
        }

        public async Task<Room> UpdateAsync(int id, CreateRoomDto room)
        {
            if (room == null)
            {
                _logger.LogWarning("Oda güncelleme başarısız: DTO boş.");
                throw new ArgumentNullException(nameof(room), "Oda bilgisi boş olamaz.");
            }

            bool exists = await _roomRepository.AnyOtherRoomExistsAsync(id, room.RoomCode, room.Description);
            if (exists)
            {
                _logger.LogWarning("Oda güncelleme başarısız: '{RoomCode}' çakışması.", room.RoomCode);
                throw new InvalidOperationException("Bu salon numarası ve açıklama kombinasyonuna sahip bir oda zaten mevcut.");
            }

            var roomToUpdate = new Room
            {
                RoomCode = room.RoomCode,
                Description = room.Description
            };

            var updatedRoom = await _roomRepository.UpdateAsync(id, roomToUpdate);
            if (updatedRoom == null)
            {
                _logger.LogWarning("Oda güncelleme başarısız: ID {Id} bulunamadı.", id);
                throw new KeyNotFoundException($"Güncellenecek ID {id} olan oda bulunamadı.");
            }

            _logger.LogInformation("Oda güncellendi. ID: {Id}", id);

            return updatedRoom;
        }
    }
}