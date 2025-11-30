using LibrarySystem.API.DataContext;
using LibrarySystem.API.Dtos.BookDtos;
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
                        SELECT TOP (10) * FROM Categories 
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
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryResultDto>> GetAllCategoriesAsync()
        {
            var query = _context.Categories
                .GroupJoin(_context.Books,
                    category => category.Id,
                    book => book.CategoryId,
                    (category, books) => new
                    {
                        Category = category,
                        BookCount = books.Count()
                    })
                .OrderBy(x => x.Category.Name);

            var items = await query
                .Select(x => new CategoryResultDto
                {
                    Id = x.Category.Id,
                    Name = x.Category.Name,
                    BookCount = x.BookCount 
                })
                .ToListAsync();

            return items;
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

        public async Task<PaginatedCategoryResult<CategoryResultDto>> GetAllCategoriesPageableAsync(int page, int pageSize)
        {
            var totalCount = await _context.Categories.CountAsync();

            int skipCount = (page - 1) * pageSize;

            var query = _context.Categories
                .GroupJoin(_context.Books,
                    category => category.Id,
                    book => book.CategoryId,
                    (category, books) => new
                    {
                        Category = category,
                        BookCount = books.Count()
                    })
                .OrderBy(x => x.Category.Name)
                .Skip(skipCount)
                .Take(pageSize);

            var items = await query
                .Select(x => new CategoryResultDto
                {
                    Id = x.Category.Id,
                    Name = x.Category.Name,
                    BookCount = x.BookCount 
                })
                .ToListAsync();

            return new PaginatedCategoryResult<CategoryResultDto>(
                items,
                totalCount,
                page,
                pageSize
            );
        }

        public async Task<Category?> UpdateCategoryAsync(int id, Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(id);

            if (existingCategory == null)
            {
                return null;
            }

            existingCategory.Name = category.Name;

            await _context.SaveChangesAsync();
            return existingCategory;
        }
    }
}
