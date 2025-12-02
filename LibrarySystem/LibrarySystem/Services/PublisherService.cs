using LibrarySystem.API.Dtos.PublisherDtos;
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

        public async Task<Publisher> AddPublisherAsync(CreatePublisherDto publisher)
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

            var publisherEntity = new Publisher
            {
                Name = publisher.Name
            };

            var addedPublisher = await _publisherRepository.AddAsync(publisherEntity);

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
        public async Task<IEnumerable<Publisher>> GetByNameAsync(string name)
        {
            _logger.LogInformation("'{Name}' adlı yayınevi için sorgulama isteği alındı.", name);
            var publishers = await _publisherRepository.GetByNameAsync(name);

            return publishers;
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
                var publishers = await _publisherRepository.GetByNameAsync(name);
                var existingPublisher = publishers.FirstOrDefault();

                if (existingPublisher != null)
                {
                    return existingPublisher;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByNameAsync çağrılırken beklenmeyen bir hata oluştu.");
                throw;
            }

            _logger.LogInformation("GetOrCreate: '{Name}' bulunamadı, yeni kayıt oluşturuluyor.", name);

            return await AddPublisherAsync(new CreatePublisherDto { Name = name });
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

        public async Task<PaginatedPublisherResult<Publisher>> GetAllPublisherPageableAsync(int page, int pageSize)
        {
            _logger.LogInformation("Sayfalı yayınevi listeleme isteği alındı. Sayfa: {Page}, Sayfa Boyutu: {PageSize}", page, pageSize);

            var publishersResult = await _publisherRepository.GetAllPublisherPageableAsync(page, pageSize);

            _logger.LogInformation(
                "Sayfalı yayınevi listeleme tamamlandı. Toplam Yayınevi: {TotalCount}, Toplam Sayfa: {TotalPages}",
                publishersResult.TotalCount,
                publishersResult.TotalPages
            );

            return publishersResult;
        }

        public async Task<Publisher?> UpdatePublisherAsync(int id, UpdatePublisherDto publisherDto)
        {
            _logger.LogInformation("Yayınevi güncelleme isteği alındı. ID: {PublisherId}", id);

            var publisherData = new Publisher
            {
                Name = publisherDto.Name
            };

            var updatedPublisher = await _publisherRepository.UpdatePublisherAsync(id, publisherData);

            if (updatedPublisher == null)
            {
                _logger.LogWarning("ID: {PublisherId} olan yayınevi bulunamadı.", id);
                throw new KeyNotFoundException($"ID: {id} olan yayınevi bulunamadı.");
            }

            _logger.LogInformation("Yayınevi başarıyla güncellendi. ID: {PublisherId}", id);

            return updatedPublisher;
        }
    }

}