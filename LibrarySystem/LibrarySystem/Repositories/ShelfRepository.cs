using LibrarySystem.API.DataContext;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class ShelfRepository : IShelfRepository
    {
        private readonly AppDbContext _context;
        public ShelfRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Shelf> AddShelfAsync(Shelf shelf)
        {
            var added = await _context.Shelves.AddAsync(shelf);

            await _context.SaveChangesAsync();

            return added.Entity;
        }

        public async Task<Shelf?> GetShelfByCodeAndRoomIdAsync(string shelfCode, int roomId)
        {
            var shelf = await _context.Shelves
                    .FirstOrDefaultAsync(s => s.ShelfCode == shelfCode && s.RoomId == roomId);

            return shelf;
        }
    }
}
