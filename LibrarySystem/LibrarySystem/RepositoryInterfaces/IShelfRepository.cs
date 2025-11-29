using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IShelfRepository
    {
        Task<IEnumerable<Shelf>> GetShelvesByRoomIdAsync(int roomId);
        Task<Shelf?> GetShelfByIdAsync(int shelfId);
        Task<Shelf> UpdateShelfAsync(Shelf shelf);
        Task<Shelf?> GetShelfByCodeAndRoomIdAsync(string shelfCode, int roomId);
        Task<Shelf> AddShelfAsync(Shelf shelf);
    }
}
