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

        public async Task<bool> DeletePublisherByIdAsync(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);

            if (publisher == null)
                return false;

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Publisher>> GetAllAsync()
        {
            return await _context.Publishers
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Publisher?> GetByIdAsync(int id)
        {
            return await _context.Publishers.FindAsync(id);
        }

        public async Task<Publisher?> GetByNameAsync(string name)
        {

            var searchPattern = $"%{name}%";

            var result = await _context.Publishers
                 .FromSql($@"
                    SELECT * FROM Publishers 
                    WHERE 
                        DIFFERENCE(Name, {name}) >= 3  -- Yazım hatası olsa bile harf benzerliği yüksek olanları bulur.
                        OR SOUNDEX(Name) = SOUNDEX({name})  -- Yazılışı farklı ama okunuşu/tınısı aynı olanları eşleştirir.
                        OR Name LIKE {searchPattern} -- Aranan kelimenin ismin herhangi bir yerinde geçip geçmediğine bakar.
                ")
                    .FirstOrDefaultAsync();

            return result;
        }
    }
}
