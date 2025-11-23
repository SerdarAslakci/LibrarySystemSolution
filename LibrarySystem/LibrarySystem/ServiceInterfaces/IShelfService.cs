using LibrarySystem.API.Dtos.ShelfDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IShelfService
    {
        Task<Shelf?> GetShelfByCodeAndRoomIdAsync(string shelfCode, int roomId);
        Task<Shelf> AddShelfAsync(CreateShelfDto shelfDto);
    }
}
