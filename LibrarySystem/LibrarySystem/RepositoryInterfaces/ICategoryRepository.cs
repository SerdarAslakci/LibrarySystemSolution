using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryResultDto>> GetAllCategoriesAsync();
        Task<bool> IsExistsAsync(string? name);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByNameAsync(string name);
    }
}
