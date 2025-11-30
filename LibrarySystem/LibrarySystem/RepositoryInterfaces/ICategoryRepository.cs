using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryResultDto>> GetAllCategoriesAsync();
        Task<PaginatedCategoryResult<CategoryResultDto>> GetAllCategoriesPageableAsync(int page, int pageSize);
        Task<bool> IsExistsAsync(string? name);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetByNameAsync(string name);
        Task<bool> DeleteCategoryByIdAsync(int id);
        Task<Category?> UpdateCategoryAsync(int id, Category category);
    }
}
