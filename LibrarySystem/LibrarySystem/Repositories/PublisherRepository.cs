using LibrarySystem.API.DataContext;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LibrarySystem.API.Repositories
{
    public class PublisherRepository : IPublisherRepository
    {
        private readonly AppDbContext _context;

        public PublisherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Publisher> AddAsync(Publisher publisher)
        {
            var added = await _context.Publishers.AddAsync(publisher);

            await _context.SaveChangesAsync();

            return added.Entity;
        }

        public async Task<bool> AnyAsync(string name)
        {
            var lowerName = name.ToLower();
            return await _context.Publishers.AnyAsync(p => p.Name.ToLower().Contains(lowerName));
        }

        public async Task<Publisher?> GetByIdAsync(int id)
        {
            return await _context.Publishers.FindAsync(id);
        }

        public async Task<Publisher?> GetByNameAsync(string name)
        {
            return await _context.Publishers
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
        }
    }
}
