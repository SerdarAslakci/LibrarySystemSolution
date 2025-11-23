using LibrarySystem.API.Dtos.RoomDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IRoomService
    {
        Task<List<Room>> GetAllAsync();
        Task<Room?> GetRoomByIdAsync(int id);
        Task<Room> AddAsync(CreateRoomDto room);
        Task<Room> UpdateAsync(int id, CreateRoomDto room);
    }
}
