using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var exists = await IsExistsAsync(category.Name);
            if (exists)
                throw new InvalidOperationException("Bu kategori zaten mevcut.");

            await _categoryRepository.AddCategoryAsync(category);

            return category;
        }

        public async Task<bool> IsExistsAsync(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _categoryRepository.IsExistsAsync(name);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"ID'si {id} olan kategori bulunamadı.");

            return category;
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            var category = await _categoryRepository.GetByNameAsync(name);
            if (category == null)
                throw new KeyNotFoundException($"Adı '{name}' olan kategori bulunamadı.");

            return category;
        }

        public async Task<Category> GetOrCreateAsync(int? id, string? name)
        {
            if (id.HasValue)
            {
                return await GetByIdAsync(id.Value);
            }

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Kategori adı boş olamaz.");

            try
            {
                return await GetByNameAsync(name);
            }
            catch (KeyNotFoundException)
            {
                return await AddCategoryAsync(new Category { Name = name });
            }
        }

    }


}
