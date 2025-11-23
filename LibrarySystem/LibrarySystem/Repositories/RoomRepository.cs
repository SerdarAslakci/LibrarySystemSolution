using LibrarySystem.API.DataContext;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;

        public RoomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string roomCode, string description)
        {
            return await _context.Rooms
                .AnyAsync(r => r.RoomCode == roomCode || r.Description == description);
        }

        public async Task<Room?> AddAsync(Room room)
        {
            var added = await _context.Rooms.AddAsync(room);

            await _context.SaveChangesAsync();

            return added.Entity;
        }

        public async Task<List<Room>> GetAllAsync()
        {
            var rooms = await _context.Rooms.ToListAsync();

            return rooms;
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public async Task<Room?> UpdateAsync(int id,Room room)
        {
            var existingRoom = await _context.Rooms.FindAsync(id);

            if (existingRoom == null)
            {
                return null;
            }

            existingRoom.RoomCode = room.RoomCode;
            existingRoom.Description = room.Description;

            await _context.SaveChangesAsync();

            return existingRoom;
        }
        public async Task<bool> AnyOtherRoomExistsAsync(int id, string roomCode, string description)
        {
            return await _context.Rooms
                .AnyAsync(r => r.Id != id && r.RoomCode == roomCode && r.Description == description);
        }
    }
}
