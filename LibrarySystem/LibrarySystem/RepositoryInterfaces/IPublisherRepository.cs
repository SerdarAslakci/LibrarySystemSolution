using LibrarySystem.Models.Models;
using System.Linq.Expressions;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IPublisherRepository
    {
        Task<Publisher> AddAsync(Publisher publisher);
        Task<bool> AnyAsync(string name);
        Task<Publisher?> GetByIdAsync(int id);
        Task<Publisher?> GetByNameAsync(string name);
        Task<IEnumerable<Publisher>> GetAllAsync();
    }
}
