using LibrarySystem.API.Dtos.BookDtos;
using LibrarySystem.API.Dtos.UserDtos;
using LibrarySystem.Models.Models;

namespace LibrarySystem.API.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<PaginatedResult<UserViewDto>> GetUsersWithFilterAsync(UserFilterDto filter);
        Task<UserViewDto?> GetUserByIdAsync(string userId);
        Task<UserViewDto?> GetUserByEmailAsync(string email);
        Task<int> GetUserCountAsync();
    }
}
