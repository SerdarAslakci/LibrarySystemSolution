using LibrarySystem.API.Dtos.CategoryDtos;
using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            _logger.LogInformation("Yeni kategori ekleme işlemi başlatıldı: {CategoryName}", category?.Name);

            if (category == null)
            {
                _logger.LogWarning("Kategori ekleme başarısız: Kategori nesnesi null.");
                throw new ArgumentNullException(nameof(category));
            }

            var exists = await IsExistsAsync(category.Name);
            if (exists)
            {
                _logger.LogWarning("Kategori ekleme başarısız: '{CategoryName}' zaten mevcut.", category.Name);
                throw new InvalidOperationException("Bu kategori zaten mevcut.");
            }

            await _categoryRepository.AddCategoryAsync(category);

            _logger.LogInformation("Kategori başarıyla eklendi. ID: {Id}, İsim: {Name}", category.Id, category.Name);

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
            {
                _logger.LogWarning("Kategori sorgulama başarısız: ID {Id} bulunamadı.", id);
                throw new KeyNotFoundException($"ID'si {id} olan kategori bulunamadı.");
            }

            return category;
        }

        public async Task<IEnumerable<Category>> GetByNameAsync(string name)
        {
            _logger.LogInformation("Kategori adıyla sorgulama başlatıldı: {CategoryName}", name);
            var categorys = await _categoryRepository.GetByNameAsync(name);

            _logger.LogInformation("{Count} adet kategori bulundu isimle: {CategoryName}", categorys.Count(), name);
            return categorys;
        }

        public async Task<Category> GetOrCreateAsync(int? id, string? name)
        {
            if (id.HasValue)
            {
                return await GetByIdAsync(id.Value);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetOrCreate başarısız: İsim parametresi boş.");
                throw new ArgumentException("Kategori adı boş olamaz.");
            }

            try
            {
                var existingCategory = (await GetByNameAsync(name)).FirstOrDefault();

                if (existingCategory != null)
                {
                    return existingCategory;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByNameAsync çağrılırken beklenmeyen bir hata oluştu.");
                throw;
            }

            _logger.LogInformation("GetOrCreate: '{Name}' bulunamadı, yeni oluşturuluyor.", name);

            return await AddCategoryAsync(new Category { Name = name });
        }

        public async Task<IEnumerable<CategoryResultDto>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Kategori listesi getiriliyor...");

                var categories = await _categoryRepository.GetAllCategoriesAsync();

                if (!categories.Any())
                {
                    _logger.LogWarning("Kategori listesi boş döndü.");
                }
                else
                {
                    _logger.LogInformation("{Count} kategori başarıyla getirildi.", categories.Count());
                }

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori listesini getirirken bir hata oluştu.");
                throw;
            }
        }

        public async Task<bool> DeleteCategoryByIdAsync(int id)
        {
            _logger.LogInformation("Kategori silme işlemi başlatıldı. ID: {CategoryId}", id);

            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                _logger.LogWarning("Silme başarısız: ID'si {Id} olan kategori bulunamadı.", id);
                throw new KeyNotFoundException($"ID'si {id} olan kategori bulunamadı.");
            }

            await _categoryRepository.DeleteCategoryByIdAsync(id);

            _logger.LogInformation("Kategori başarıyla silindi. ID: {CategoryId}", id);

            return true;
        }

        public async Task<PaginatedCategoryResult<CategoryResultDto>> GetAllCategoriesPageableAsync(int page, int pageSize)
        {
            _logger.LogInformation("Sayfalı kategori listesi getiriliyor. Sayfa: {Page}, Sayfa Boyutu: {PageSize}", page, pageSize);

            var categories = await _categoryRepository.GetAllCategoriesPageableAsync(page, pageSize);

            _logger.LogInformation("{Count} kategori başarıyla getirildi. Sayfa: {Page}", categories.Items.Count, page);

            return categories;
        }

        public async Task<Category?> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto)
        {
            _logger.LogInformation("Kategori güncelleme işlemi başladı. ID: {Id}", id);

            var existingCategory = await _categoryRepository.GetByIdAsync(id);

            if (existingCategory == null)
            {
                _logger.LogWarning("ID: {Id} olan kategori bulunamadı.", id);
                throw new KeyNotFoundException($"ID: {id} olan kategori bulunamadı.");
            }

            var categories = await _categoryRepository.GetByNameAsync(categoryDto.Name);

            var sameNameCategory = categories
                .FirstOrDefault(x => x.Name.Equals(categoryDto.Name, StringComparison.OrdinalIgnoreCase));

            if (sameNameCategory != null)
            {
                _logger.LogError("Güncelleme başarısız: Aynı isimde kategori zaten mevcut. İsim: {CategoryName}", categoryDto.Name);
                throw new InvalidOperationException("Aynı isimde kategori zaten mevcut.");
            }

            existingCategory.Name = categoryDto.Name;

            var updatedCategory = await _categoryRepository.UpdateCategoryAsync(id, existingCategory);

            _logger.LogInformation("Kategori başarıyla güncellendi. ID: {Id}", id);

            return updatedCategory;
        }
    }
}