using LibrarySystem.API.Dtos.ShelfDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IShelfService
    {
        Task<Shelf> GetShelfByIdAsync(int shelfId);
        Task<IEnumerable<Shelf>> GetShelvesByRoomIdAsync(int roomId);
        Task<Shelf?> GetShelfByCodeAndRoomIdAsync(string shelfCode, int roomId);
        Task<Shelf> UpdateShelfAsync(int id, UpdateShelfDto shelfDto);
        Task<Shelf> AddShelfAsync(CreateShelfDto shelfDto);
    }
}
