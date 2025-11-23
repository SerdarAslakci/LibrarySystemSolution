using LibrarySystem.API.Dtos.RoomDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Services
{
    public class RoomService : IRoomService
    {

        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<Room> AddAsync(CreateRoomDto room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room), "Oda bilgisi boş olamaz.");

            if (string.IsNullOrWhiteSpace(room.RoomCode))
                throw new ArgumentException("RoomCode boş olamaz.");

            if (string.IsNullOrWhiteSpace(room.Description))
                throw new ArgumentException("Description boş olamaz.");

            bool exists = await _roomRepository.ExistsAsync(room.RoomCode, room.Description);
            if (exists)
                throw new InvalidOperationException("Bu salon numarası ve açıklama kombinasyonuna sahip bir oda zaten mevcut.");

            var roomToAdd = new Room
            {
                RoomCode = room.RoomCode,
                Description = room.Description
            };

            var addedRoom = await _roomRepository.AddAsync(roomToAdd);
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
                throw new KeyNotFoundException($"ID'si {id} olan oda bulunamadı.");

            return room;
        }

        public async Task<Room> UpdateAsync(int id, CreateRoomDto room)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room), "Oda bilgisi boş olamaz.");

            bool exists = await _roomRepository.AnyOtherRoomExistsAsync(id, room.RoomCode, room.Description);
            if (exists)
                throw new InvalidOperationException("Bu salon numarası ve açıklama kombinasyonuna sahip bir oda zaten mevcut.");

            var roomToUpdate = new Room
            {
                RoomCode = room.RoomCode,
                Description = room.Description
            };

            var updatedRoom = await _roomRepository.UpdateAsync(id, roomToUpdate);
            if (updatedRoom == null)
                throw new KeyNotFoundException($"Güncellenecek ID {id} olan oda bulunamadı.");

            return updatedRoom;
        }
    }
}
