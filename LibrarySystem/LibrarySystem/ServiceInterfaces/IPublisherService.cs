using LibrarySystem.API.Dtos.PublisherDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IPublisherService
    {
        Task<Publisher> AddPublisherAsync(CreatePublisherDto publisherDto);
        Task<Publisher?> UpdatePublisherAsync(int id, UpdatePublisherDto publisherDto);
        Task<PaginatedPublisherResult<Publisher>> GetAllPublisherPageableAsync(int page, int pageSize);
        Task<bool> IsExistsAsync(string? name);
        Task<IEnumerable<Publisher>> GetAllAsync();
        Task<Publisher?> GetByIdAsync(int id);
        Task<IEnumerable<Publisher>> GetByNameAsync(string name);
        Task<Publisher> GetOrCreateAsync(int? id, string? name);
        Task<bool> DeletePublisherByIdAsync(int id);

    }
}
