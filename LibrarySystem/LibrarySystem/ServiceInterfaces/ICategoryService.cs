using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface ICategoryService
    {
        Task<bool> IsExistsAsync(string? name);
        Task<Category> AddCategoryAsync(Category category);
        Task<Category?> GetByIdAsync(int id);
        Task<IEnumerable<Category>> GetByNameAsync(string name);
        Task<Category> GetOrCreateAsync(int? id, string? name);
        Task<IEnumerable<CategoryResultDto>> GetAllCategoriesAsync();
        Task<PaginatedCategoryResult<CategoryResultDto>> GetAllCategoriesPageableAsync(int page, int pageSize);
        Task<Category?> UpdateCategoryAsync(int id, UpdateCategoryDto category);
        Task<bool> DeleteCategoryByIdAsync(int id);
    }
}
