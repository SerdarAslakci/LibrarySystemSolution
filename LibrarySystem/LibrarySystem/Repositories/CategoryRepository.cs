using LibrarySystem.API.DataContext;
using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Category> AddCategoryAsync(Category category)
        {
            var addedCategory = await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return addedCategory.Entity;
        }

        public async Task<bool> IsExistsAsync(string? name)
        {
            return await _context.Categories.AnyAsync(c =>
                c.Name != null &&
                c.Name.ToLower() == (name ?? string.Empty).ToLower());
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

 public async Task<IEnumerable<Category>> GetByNameAsync(string name)
{
    var searchPattern = $"%{name}%";

    return await _context.Categories
        .FromSqlInterpolated($@"
                SELECT * FROM Categories 
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

        public async Task<IEnumerable<CategoryResultDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryResultDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    BookCount = _context.Books.Count(b => b.CategoryId == c.Id)
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
