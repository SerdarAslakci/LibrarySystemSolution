using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;
using Microsoft.Extensions.Logging;

namespace LibrarySystem.API.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;
        private readonly ILogger<PublisherService> _logger;

        public PublisherService(IPublisherRepository publisherRepository, ILogger<PublisherService> logger)
        {
            _publisherRepository = publisherRepository;
            _logger = logger;
        }

        public async Task<Publisher> AddPublisherAsync(Publisher publisher)
        {
            _logger.LogInformation("Yeni yayınevi ekleme isteği: {PublisherName}", publisher?.Name);

            if (publisher == null)
            {
                _logger.LogWarning("Yayınevi ekleme başarısız: Publisher nesnesi null.");
                throw new ArgumentNullException(nameof(publisher));
            }

            var exists = await IsExistsAsync(publisher.Name);
            if (exists)
            {
                _logger.LogWarning("Yayınevi ekleme başarısız: '{PublisherName}' zaten mevcut.", publisher.Name);
                throw new InvalidOperationException("Bu yayınevi zaten mevcut.");
            }

            var addedPublisher = await _publisherRepository.AddAsync(publisher);

            _logger.LogInformation("Yayınevi başarıyla eklendi. ID: {Id}, İsim: {Name}", addedPublisher.Id, addedPublisher.Name);

            return addedPublisher;
        }

        public async Task<bool> IsExistsAsync(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await _publisherRepository.AnyAsync(name);
        }

        public async Task<Publisher> GetByIdAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id);

            if (publisher == null)
            {
                _logger.LogWarning("Yayınevi sorgulama başarısız: ID {Id} bulunamadı.", id);
                throw new KeyNotFoundException($"ID'si {id} olan yayınevi bulunamadı.");
            }
            return publisher;
        }
        public async Task<Publisher> GetByNameAsync(string name)
        {
            var publisher = await _publisherRepository.GetByNameAsync(name);

            if (publisher == null)
            {
                _logger.LogWarning("Yayınevi sorgulama başarısız: '{Name}' bulunamadı.", name);
                throw new KeyNotFoundException($"Adı '{name}' olan yayınevi bulunamadı.");
            }
            return publisher;
        }

        public async Task<Publisher> GetOrCreateAsync(int? id, string? name)
        {
            if (id.HasValue)
            {
                return await GetByIdAsync(id.Value);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetOrCreate başarısız: İsim parametresi boş.");
                throw new ArgumentException("Yayınevi adı boş olamaz.");
            }

            try
            {
                return await GetByNameAsync(name);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation("GetOrCreate: '{Name}' bulunamadı, yeni kayıt oluşturuluyor.", name);
                return await AddPublisherAsync(new Publisher { Name = name });
            }
        }

        public async Task<IEnumerable<Publisher>> GetAllAsync()
        {
            _logger.LogInformation("Tüm yayınevleri için listeleme isteği alındı.");

            var publishers = await _publisherRepository.GetAllAsync();

            if (publishers == null || !publishers.Any())
            {
                _logger.LogWarning("Yayınevi listeleme: Kayıt bulunamadı.");
            }
            else
            {
                _logger.LogInformation("Toplam {Count} yayınevi listelendi.", publishers.Count());
            }

            return publishers ?? Enumerable.Empty<Publisher>();
        }

        public async Task<bool> DeletePublisherByIdAsync(int id)
        {
            _logger.LogInformation("Yayınevi silme işlemi başlatıldı. ID: {PublisherId}", id);

            var publisher = await _publisherRepository.GetByIdAsync(id);

            if (publisher == null)
            {
                _logger.LogWarning("Silme başarısız: ID'si {Id} olan yayınevi bulunamadı.", id);
                throw new KeyNotFoundException($"ID'si {id} olan yayınevi bulunamadı.");
            }

            await _publisherRepository.DeletePublisherByIdAsync(id);

            _logger.LogInformation("Yayınevi başarıyla silindi. ID: {PublisherId}", id);

            return true;
        }
    }

}