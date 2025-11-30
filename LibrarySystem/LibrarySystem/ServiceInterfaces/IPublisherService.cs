using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IPublisherService
    {
        Task<Publisher> AddPublisherAsync(Publisher publisher);
        Task<bool> IsExistsAsync(string? name);
        Task<IEnumerable<Publisher>> GetAllAsync();
        Task<Publisher?> GetByIdAsync(int id);
        Task<IEnumerable<Publisher>> GetByNameAsync(string name);
        Task<Publisher> GetOrCreateAsync(int? id, string? name);
        Task<bool> DeletePublisherByIdAsync(int id);

    }
}
