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

        public async Task<IEnumerable<Publisher>> GetByNameAsync(string name)
        {
            var searchPattern = $"%{name}%";

            return await _context.Publishers
                .FromSqlInterpolated($@"
                    SELECT * FROM Publishers 
                    WHERE 
                        DIFFERENCE(Name, {name}) >= 3 
                        OR SOUNDEX(Name) = SOUNDEX({name}) 
                        OR Name LIKE {searchPattern}
                    ORDER BY 
                        CASE 
                            WHEN Name = {name} THEN 1             -- Tam eşleşme en üstte
                            WHEN Name LIKE {searchPattern} THEN 2 -- İçinde geçenler ikinci sırada
                            ELSE 3                                -- Sadece ses benzerliği olanlar en altta
                        END
                ")
                .ToListAsync();
        }
    }
}
