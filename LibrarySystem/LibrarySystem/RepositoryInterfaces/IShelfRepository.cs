using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IShelfRepository
    {
        Task<Shelf?> GetShelfByCodeAndRoomIdAsync(string shelfCode, int roomId);
        Task<Shelf> AddShelfAsync(Shelf shelf);
    }
}
