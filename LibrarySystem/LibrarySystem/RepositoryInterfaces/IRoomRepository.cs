using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IRoomRepository
    {
        Task<List<Room>> GetAllAsync();
        Task<bool> ExistsAsync(string roomCode, string description);
        Task<Room?> GetRoomByIdAsync(int id);
        Task<Room?> AddAsync(Room room);
        Task<bool> AnyOtherRoomExistsAsync(int id, string roomCode, string description);
        Task<Room?> UpdateAsync(int id, Room room);
    }
}
