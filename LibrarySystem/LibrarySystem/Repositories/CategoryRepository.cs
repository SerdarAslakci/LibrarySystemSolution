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

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Name != null &&
                    c.Name.ToLower() == name.ToLower());
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
