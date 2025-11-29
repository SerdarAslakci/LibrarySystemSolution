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
                    .Include(s => s.Room)
                    .FirstOrDefaultAsync(s => s.ShelfCode == shelfCode && s.RoomId == roomId);

            return shelf;
        }

        public async Task<Shelf?> GetShelfByIdAsync(int shelfId)
        {
            return await _context.Shelves
                .Include(s => s.Room)
                .FirstOrDefaultAsync(x => x.Id == shelfId);
        }

        public async Task<IEnumerable<Shelf>> GetShelvesByRoomIdAsync(int roomId)
        {
            return await _context.Shelves
                    .Include(s => s.Room)
                    .Where(s => s.RoomId == roomId)
                    .ToListAsync();
        }

        public async Task<Shelf> UpdateShelfAsync(Shelf shelf)
        {
            var updated =  _context.Shelves.Update(shelf);
            await _context.SaveChangesAsync();

            return updated.Entity;
        }
    }
}
