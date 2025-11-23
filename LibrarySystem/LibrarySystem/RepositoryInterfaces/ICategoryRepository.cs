using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface ICategoryRepository
    {
        Task<bool> IsExistsAsync(string? name);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByNameAsync(string name);
    }
}
