using LibrarySystem.API.RepositoryInterfaces;
using LibrarySystem.API.ServiceInterfaces;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly IPublisherRepository _publisherRepository;

        public PublisherService(IPublisherRepository publisherRepository)
        {
            _publisherRepository = publisherRepository;
        }

        public async Task<Publisher> AddPublisherAsync(Publisher publisher)
        {
            if (publisher == null)
                throw new ArgumentNullException(nameof(publisher));

            var exists = await IsExistsAsync(publisher.Name);
            if (exists)
                throw new InvalidOperationException("Bu yayınevi zaten mevcut.");

            var addedPublisher = await _publisherRepository.AddAsync(publisher);

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
                throw new KeyNotFoundException($"ID'si {id} olan yayınevi bulunamadı.");
            }
            return publisher;
        }
        public async Task<Publisher> GetByNameAsync(string name)
        {
            var publisher = await _publisherRepository.GetByNameAsync(name);

            if (publisher == null)
            {
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
                throw new ArgumentException("Yayınevi adı boş olamaz.");

            try
            {
                return await GetByNameAsync(name);
            }
            catch (KeyNotFoundException)
            {
                return await AddPublisherAsync(new Publisher { Name = name });
            }
        }

    }

}
