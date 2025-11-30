using LibrarySystem.API.Dtos.AuthorDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAuthorsAsync();
        Task<PaginatedAuthorResult<Author>> GetAllAuthorsPageableAsync(int page, int pageSize);
        Task<IEnumerable<Author>> GetAuthorsByNameAsync(string firstName, string lastName);
        Task<Author?> GetByIdAsync(int id);
        Task<Author?> GetByNameAsync(string firstName, string lastName);
        Task<Author> AddAuthorAsync(Author author);
        Task<bool> IsExistsAsync(string? firstName, string? lastName);
        Task<bool> DeleteAuthorByIdAsync(int id);

    }
}
