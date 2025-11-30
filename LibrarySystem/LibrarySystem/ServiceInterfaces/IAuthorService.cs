using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.ServiceInterfaces
{
    public interface IAuthorService
    {
        Task<Author> AddAuthorAsync(CreateAuthorDto authorDto);
        Task<bool> IsExistsAsync(string? firstName, string? lastName);
        Task<IEnumerable<Author>> GetAllAuthorsAsync();
        Task<PaginatedAuthorResult<Author>> GetAllAuthorsPageableAsync(int page, int pageSize);
        Task<IEnumerable<Author>> GetAuthorsByNameAsync(string firstName, string lastName);
        Task<Author?> GetByIdAsync(int id);
        Task<Author?> GetByNameAsync(string firstName, string lastName);
        Task<Author?> GetOrCreateAsync(int? id, string? firstName, string? lastName);
        Task<bool> DeleteAuthorByIdAsync(int id);

    }
}
