using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface ICategoryService
    {
        Task<bool> IsExistsAsync(string? name);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByNameAsync(string name);
        Task<Category> GetOrCreateAsync(int? id, string? name);
        Task<IEnumerable<CategoryResultDto>> GetAllCategoriesAsync();
    }
}
